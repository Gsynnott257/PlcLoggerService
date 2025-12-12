using PlcLoggerService.Models;
namespace PlcLoggerService.Services;
public sealed class PlcLoggerWorker : BackgroundService
{
    private readonly ILogger<PlcLoggerWorker> _log;
    private readonly IConfiguration _cfg;
    private readonly SqlWriter _sql;
    private readonly DbConfigLoader _loader;
    private readonly Dictionary<string, int> _groupPeriods;
    private readonly TimeSpan _configReloadInterval = TimeSpan.FromMinutes(5);
    private DateTime _nextReloadUtc = DateTime.MinValue;
    private List<PlcTag>? _cachedTags;
    private List<PlcEndpoint>? _cachedEndpoints;
    public PlcLoggerWorker(ILogger<PlcLoggerWorker> log, IConfiguration cfg)
    {
        _log = log;
        _cfg = cfg;
        _sql = new SqlWriter(_cfg.GetConnectionString("Db")!);
        _loader = new DbConfigLoader(_cfg.GetConnectionString("Db")!);
        _groupPeriods = _cfg.GetSection("ScanGroups").Get<Dictionary<string, int>>() ?? new() { ["slow"] = 5000, ["normal"] = 500, ["fast"] = 100 };
    }
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _log.LogInformation("PlcLoggerWorker started.");
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await LoadConfigIfDueAsync(ct);
                var endpoints = _cachedEndpoints!;
                var tags = _cachedTags!;
                var byGroup = tags.GroupBy(t => t.ScanGroup);
                foreach (var group in byGroup)
                    await RunOneGroupCycleAsync(_groupPeriods[group.Key], endpoints, [.. group], ct);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Top-level cycle failed.");
            }
            await Task.Delay(250, ct);
        }
        _log.LogInformation("PlcLoggerWorker stopping.");
    }
    private async Task LoadConfigIfDueAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        if (_cachedEndpoints is null || _cachedTags is null || now >= _nextReloadUtc)
        {
            var (endpoints, tags) = await _loader.LoadAsync(ct);
            _cachedEndpoints = endpoints;
            _cachedTags = tags;
            _nextReloadUtc = now + _configReloadInterval;
            _log.LogInformation("Configuration reloaded. Next reload at {nextReloadUtc}", _nextReloadUtc);
        }
    }
    private async Task RunOneGroupCycleAsync(int periodMs, List<PlcEndpoint> endpoints, List<PlcTag> tags, CancellationToken ct)
    {
        foreach (var ep in endpoints.Where(e => e.Enabled))
        {
            var path = DbConfigLoader.ComputePath(ep.Slot);
            var epTags = tags.Where(t => t.PlcId == ep.PlcId && t.Enabled).ToList();
            var singleTags = epTags.Where(t => !t.IsArray).ToList();
            var arrayTags = epTags.Where(t => t.IsArray).ToList();
            foreach (var tag in singleTags)
            {
                var val = Plc.LibPlcTagReader.ReadValue(tag.DataType, tag.Address, ep.IpAddress, path);
                if (val is null) continue;
                var now = DateTime.UtcNow;
                double? curr = val is IConvertible ? Convert.ToDouble(val) : null;
                var (last, lastTs) = await _sql.GetLastAsync(tag.TagId, ct);
                if (ChangeDetection.ShouldLog(curr, last, tag.Deadband, lastTs, tag.MinMs, tag.MaxMs, now))
                {
                    await _sql.InsertEventAsync(ep.PlcId, tag, val, curr, "Good", now, ct);
                    await _sql.UpdateLastAsync(tag.TagId, val, curr, ct);
                }
            }
            var arraysByName = arrayTags.GroupBy(t => t.BaseArrayName);
            foreach (var arr in arraysByName)
            {
                var baseName = arr.Key;
                var elemCount = arr.First().ElemCount ?? 16;
                try
                {
                    using var reader = new Plc.BlockArrayReader(ep.IpAddress, path, baseName, elemCount);
                    var values = reader.ReadArray(elemCount, timeoutMs: 2000);
                    var now = DateTime.UtcNow;
                    foreach (var tag in arr)
                    {
                        var idxStr = tag.Address[(tag.Address.IndexOf('[') + 1)..tag.Address.IndexOf(']')];
                        int idx = int.Parse(idxStr);
                        var val = values[idx - 1];
                        double curr = val;
                        var (last, lastTs) = await _sql.GetLastAsync(tag.TagId, ct);
                        if (ChangeDetection.ShouldLog(curr, last, tag.Deadband, lastTs, tag.MinMs, tag.MaxMs, now))
                        {
                            await _sql.InsertEventAsync(ep.PlcId, tag, val, curr, "Good", now, ct);
                            await _sql.UpdateLastAsync(tag.TagId, val, curr, ct);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "Array read failed for {base} on {ip}", baseName, ep.IpAddress);
                }
            }
            await Task.Delay(periodMs, ct);
        }
    }
}