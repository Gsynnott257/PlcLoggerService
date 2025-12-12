namespace PlcLoggerService.Models;
public sealed class PlcEndpoint
{
    public int PlcId { get; set; }   // DB identity (optional if you seed via code)
    public string Name { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public int? Slot { get; set; }   // typically 0
    public bool Enabled { get; set; } = true;
    public string? Note { get; set; }
}