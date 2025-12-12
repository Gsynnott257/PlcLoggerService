using libplctag.NativeImport;
namespace PlcLoggerService.Plc
{
    public sealed class BlockArrayReader : IDisposable
    {
        private int _tag;
        public BlockArrayReader(string gateway, string path, string tagName, int elemCount = 16)
        {
            string attr = $"protocol=ab_eip&gateway={gateway}&path={path}&plc=LGX&elem_size=4&elem_count={elemCount}&name={tagName}";
            _tag = plctag.plc_tag_create(attr, 5000);
            var status = (STATUS_CODES)plctag.plc_tag_status(_tag);
            if (_tag == 0 || status != STATUS_CODES.PLCTAG_STATUS_OK)
                throw new InvalidOperationException($"plc_tag_create failed: {status}");
        }
        public int[] ReadArray(int elemCount = 16, int timeoutMs = 2000)
        {
            var rc = (STATUS_CODES)plctag.plc_tag_read(_tag, timeoutMs);
            if (rc != STATUS_CODES.PLCTAG_STATUS_OK)
                throw new InvalidOperationException($"plc_tag_read failed: {rc}");

            var result = new int[elemCount];
            for (int i = 0; i < elemCount; i++)
                result[i] = (int)plctag.plc_tag_get_uint32(_tag, i * 4);
            return result;
        }
        public void Dispose()
        {
            if (_tag != 0) plctag.plc_tag_destroy(_tag);
            _tag = 0;
            GC.SuppressFinalize(this);
        }
    }
}