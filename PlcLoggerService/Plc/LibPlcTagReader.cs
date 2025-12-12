using libplctag;
using libplctag.DataTypes.Simple;
namespace PlcLoggerService.Plc;
public static class LibPlcTagReader
{
    public static object? ReadValue(string dataType, string address, string gateway, string path, TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(5);
        switch (dataType.ToUpperInvariant())
        {
            case "DINT":
                {
                    var t = new TagDint
                    {
                        Name = address,
                        Gateway = gateway,
                        Path = path,
                        PlcType = PlcType.ControlLogix,
                        Protocol = Protocol.ab_eip,
                        Timeout = timeout.Value
                    };
                    return t.Read();
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
                    return t.Read();
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
                    return t.Read();
                }
            default:
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