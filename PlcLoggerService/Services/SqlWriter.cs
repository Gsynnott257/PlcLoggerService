using Microsoft.Data.SqlClient;
using PlcLoggerService.Models;
namespace PlcLoggerService.Services;
public sealed class SqlWriter(string connStr)
{
    public async Task<(double? last, DateTime? lastTs)> GetLastAsync(int tagId, CancellationToken ct)
    {
        using var conn = new SqlConnection(connStr);
        await conn.OpenAsync(ct);
        using var cmd = new SqlCommand("SELECT last_value_num, last_updated_utc FROM dbo.PlcTag_Last WHERE tag_id=@id", conn);
        cmd.Parameters.AddWithValue("@id", tagId);
        using var rd = await cmd.ExecuteReaderAsync(ct);
        double? v = null; DateTime? ts = null;
        if (await rd.ReadAsync(ct))
        {
            if (!rd.IsDBNull(0)) v = rd.GetDouble(0);
            if (!rd.IsDBNull(1)) ts = rd.GetDateTime(1);
        }
        return (v, ts);
    }

    public async Task UpdateLastAsync(int tagId, object? value, double? numeric, CancellationToken ct)
    {
        using var conn = new SqlConnection(connStr);
        await conn.OpenAsync(ct);
        using var cmd = new SqlCommand(@"
MERGE dbo.PlcTag_Last AS dst
USING (SELECT @tag_id AS tag_id) AS src
ON dst.tag_id = src.tag_id
WHEN MATCHED THEN UPDATE SET last_value_str=@s, last_value_num=@n, last_updated_utc=SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT (tag_id,last_value_str,last_value_num,last_updated_utc)
VALUES (@tag_id,@s,@n,SYSUTCDATETIME());", conn);
        cmd.Parameters.AddWithValue("@tag_id", tagId);
        cmd.Parameters.AddWithValue("@s", value?.ToString() ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@n", (object?)numeric ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task InsertEventAsync(int plcId, PlcTag tag, object? value, double? numeric, string quality,
                                       DateTime? srcUtc, CancellationToken ct)
    {
        using var conn = new SqlConnection(connStr);
        await conn.OpenAsync(ct);
        using var cmd = new SqlCommand(@"
INSERT INTO dbo.PlcTag_Event(plc_id,tag_id,tag_name,value_str,value_num,quality,src_ts_utc)
VALUES (@plc,@tag,@name,@s,@n,@q,@ts);", conn);
        cmd.Parameters.AddWithValue("@plc", plcId);
        cmd.Parameters.AddWithValue("@tag", tag.TagId);
        cmd.Parameters.AddWithValue("@name", tag.TagName);
        cmd.Parameters.AddWithValue("@s", value?.ToString() ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@n", (object?)numeric ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@q", quality);
        cmd.Parameters.AddWithValue("@ts", (object?)srcUtc ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}