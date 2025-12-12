namespace PlcLoggerService.Models;
public sealed class PlcTag
{
    public int TagId { get; set; }
    public int PlcId { get; set; }
    public string TagName { get; set; } = "";
    public string Address { get; set; } = "";   
    public string DataType { get; set; } = "";   
    public string ScanGroup { get; set; } = "slow";
    public double? Deadband { get; set; }
    public int? MinMs { get; set; }
    public int? MaxMs { get; set; }
    public bool Enabled { get; set; } = true;
    public bool IsArray { get; set; } = false;
    public int? ElemCount { get; set; }
    public string BaseArrayName => IsArray && Address.Contains('[') ? Address[..Address.IndexOf('[')] : Address;
}