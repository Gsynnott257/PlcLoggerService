namespace PlcLoggerService.Models;
public sealed class PlcEndpoint
{
    public int PlcId { get; set; }
    public string Name { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public int? Slot { get; set; }
    public bool Enabled { get; set; } = true;
    public string? Note { get; set; }
}