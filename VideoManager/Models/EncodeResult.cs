using System;

namespace VideoManager.Models
{
    public class EncodeResult
    {
        public bool Success { get; set; }
        public long? EncodedFileLength { get; set; }
        public TimeSpan? EncodeTime { get; set; }
    }
}
