using System;
using System.ComponentModel.DataAnnotations;

namespace VideoManager.Models
{
    public class Video
    {
        public const string Directory = "Videos";

        [Key]
        public Guid Id { get; set; }

        [Required]
        public string OriginalFileName { get; set; }

        [Required]
        public VideoStatus Status { get; set; }

        [Required]
        public string OriginalType { get; set; }

        [Required]
        public string EncodedType { get; set; } = ".mp4";

        [Required]
        public long OriginalLength { get; set; }

        public long? EncodedLength { get; set; }

        public TimeSpan? EncodeTime { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public string GetOriginalFilePath()
        {
            return $@"{Directory}\{Id}{OriginalType}";
        }

        public string GetEncodedFilePath()
        {
            return $@"{Directory}\{Id}{EncodedType}";
        }

        public override string ToString()
        {
            return $@"Guid: {Id}
                     OriginalFileName: {OriginalFileName}
                     Status: {Status}
                     OriginalType: {OriginalType}
                     EncodedType: {EncodedType}
                     OriginalLength: {OriginalLength}
                     EncodedLength: {EncodedLength}";
        }
    }

    public enum VideoStatus
    {
        Uploading,
        Uploaded,
        Queued,
        Encoding,
        Ready,
        Cancelled,
        Failed
    }
}
