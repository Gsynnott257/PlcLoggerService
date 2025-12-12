namespace PlcLoggerService.Models;
public sealed class ScanGroup 
{ 
    public string Name { get; set; } = ""; 
    public int PeriodMs { get; set; } = 1000; 
}