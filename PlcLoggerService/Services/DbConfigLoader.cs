using Microsoft.Data.SqlClient;
using PlcLoggerService.Models;
namespace PlcLoggerService.Services;
public sealed class DbConfigLoader
{
    private readonly string _connStr;
    public DbConfigLoader(string connStr) => _connStr = connStr;

    public async Task<(List<PlcEndpoint> Endpoints, List<PlcTag> Tags)> LoadAsync(CancellationToken ct)
    {
        var endpoints = new List<PlcEndpoint>();
        var tags = new List<PlcTag>();

        using var conn = new SqlConnection(_connStr);
        await conn.OpenAsync(ct);

        // Endpoints
        using (var cmd = new SqlCommand(
            "SELECT plc_id, ip_address, slot, enabled, note FROM dbo.PlcEndpoint WHERE enabled=1", conn))
        using (var rd = await cmd.ExecuteReaderAsync(ct))
        {
            while (await rd.ReadAsync(ct))
                endpoints.Add(new PlcEndpoint
                {
                    PlcId = rd.GetInt32(0),
                    IpAddress = rd.GetString(1),
                    Slot = rd.IsDBNull(2) ? (int?)null : rd.GetInt32(2),
                    Enabled = rd.GetBoolean(3),
                    Note = rd.IsDBNull(4) ? null : rd.GetString(4),
                    Name = $"Plc_{rd.GetInt32(0)}"
                });
        }

        // Tags (note the new columns)
        using (var cmd = new SqlCommand(
            @"SELECT tag_id, plc_id, tag_name, address, data_type, scan_group, deadband,
                     min_interval_ms, max_interval_ms, enabled, is_array, elem_count
              FROM dbo.PlcTag
              WHERE enabled=1", conn))
        using (var rd = await cmd.ExecuteReaderAsync(ct))
        {
            while (await rd.ReadAsync(ct))
                tags.Add(new PlcTag
                {
                    TagId = rd.GetInt32(0),
                    PlcId = rd.GetInt32(1),
                    TagName = rd.GetString(2),
                    Address = rd.GetString(3),
                    DataType = rd.GetString(4),
                    ScanGroup = rd.GetString(5),
                    Deadband = rd.IsDBNull(6) ? (double?)null : rd.GetDouble(6),
                    MinMs = rd.IsDBNull(7) ? (int?)null : rd.GetInt32(7),
                    MaxMs = rd.IsDBNull(8) ? (int?)null : rd.GetInt32(8),
                    Enabled = rd.GetBoolean(9),
                    IsArray = rd.GetBoolean(10),
                    ElemCount = rd.IsDBNull(11) ? (int?)null : rd.GetInt32(11)
                });
        }

        return (endpoints, tags);
    }

    public static string ComputePath(int? slot)
        => $"1,{slot ?? 0}"; // common backplane path for Logix (adjust per chassis)
}