namespace PlcLoggerService.Services;
public static class ChangeDetection
{
    public static bool ShouldLog(double? current, double? last, double? deadband, DateTime? lastTsUtc,
                                 int? minMs, int? maxMs, DateTime nowUtc)
    {
        bool dueToMax = lastTsUtc.HasValue && maxMs.HasValue &&
                        (nowUtc - lastTsUtc.Value).TotalMilliseconds >= maxMs.Value;

        bool changed;
        if (current.HasValue && last.HasValue && deadband.HasValue)
            changed = Math.Abs(current.Value - last.Value) >= deadband.Value;
        else
            changed = true; // non-numeric or first sample

        // Respect minimum sample spacing
        if (!changed && minMs.HasValue && lastTsUtc.HasValue &&
            (nowUtc - lastTsUtc.Value).TotalMilliseconds < minMs.Value)
            return false;

        return changed || dueToMax;
    }
}