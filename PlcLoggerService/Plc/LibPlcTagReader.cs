
using libplctag;
using libplctag.DataTypes.Simple;

namespace PlcLoggerService.Plc;

public static class LibPlcTagReader
{
    /// <summary>
    /// Reads a single tag using libplctag.NET typed tags.
    /// dataType: "DINT" | "REAL" | "BOOL" (extend as needed)
    /// </summary>
    public static object? ReadValue(string dataType, string address, string gateway, string path,
                                    TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(5);

        switch (dataType.ToUpperInvariant())
        {
            case "DINT":
                {
                    var t = new TagDint
                    {
                        Name = address,              // e.g., Program:Sta100_135_140.eDataLog_RejectReasonX_Count[1]
                        Gateway = gateway,              // PLC IP
                        Path = path,                 // e.g., "1,0"
                        PlcType = PlcType.ControlLogix, // Logix family
                        Protocol = Protocol.ab_eip,
                        Timeout = timeout.Value
                    };
                    // Initialize optional; first Read() initializes if omitted. [1](https://www.nuget.org/packages/libplctag/)
                    return t.Read(); // returns int
                }

            case "REAL":
                {
                    var t = new TagReal
                    {
                        Name = address,
                        Gateway = gateway,
                        Path = path,
                        PlcType = PlcType.ControlLogix,
                        Protocol = Protocol.ab_eip,
                        Timeout = timeout.Value
                    };
                    return t.Read(); // returns float
                }

            case "BOOL":
                {
                    var t = new TagBool
                    {
                        Name = address,
                        Gateway = gateway,
                        Path = path,
                        PlcType = PlcType.ControlLogix,
                        Protocol = Protocol.ab_eip,
                        Timeout = timeout.Value
                    };
                    return t.Read(); // returns bool
                }

            default:
                // fallback: try DINT as a common case
                var tf = new TagDint
                {
                    Name = address,
                    Gateway = gateway,
                    Path = path,
                    PlcType = PlcType.ControlLogix,
                    Protocol = Protocol.ab_eip,
                    Timeout = timeout.Value
                };
                return tf.Read();
        }
    }
}