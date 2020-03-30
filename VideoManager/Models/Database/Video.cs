using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VideoManager.Models
{
    public class Video : BaseEntity
    {
        public const string Directory = "Videos";

        [Key]
        public Guid VideoId { get; set; }

        [Required]
        public string OriginalFileName { get; set; }

        [Required]
        public VideoStatus Status { get; set; }

        [Required]
        public string OriginalType { get; set; }
        public string EncodedType { get; set; } = ".mp4";

        [Required]
        public long OriginalLength { get; set; }
        public string ThumbnailFilePath { get; set; }
        public int? DurationInSeconds { get; set; }
        public long? EncodedLength { get; set; }
        public TimeSpan? EncodeTime { get; set; }
        public ICollection<PlaylistVideo> PlaylistVideos { get; set; }

        [Required]
        public string IpAddress { get; set; }

        public string GetOriginalFilePath() => $@"{Directory}\{VideoId}{OriginalType}";
        public string GetEncodedFilePath() => $@"{Directory}\{VideoId}{EncodedType}";
        public override string ToString() => $"Guid: {VideoId}\nOriginalFileName: {OriginalFileName}";
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
