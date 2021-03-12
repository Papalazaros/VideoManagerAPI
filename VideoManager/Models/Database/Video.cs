using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace VideoManager.Models
{
    public class Video : BaseEntity
    {
        public const string Directory = "Videos";
        public string GetOriginalFilePath() => $@"{Directory}\{OriginalFileName}";
        public string GetEncodedFilePath() => $@"{Directory}\{VideoId}{EncodedType}";
        public override string ToString() => $"Id: {VideoId}\nOriginalFileName: {OriginalFileName}";

        [Key]
        public int VideoId { get; set; }

        public string OriginalFileName { get; set; }

        public VideoStatus Status { get; set; }

        public string OriginalType { get; set; }

        public string EncodedType { get; set; } = ".mp4";

        public long OriginalLength { get; set; }

        public string? ThumbnailFilePath { get; set; }

        public int? DurationInSeconds { get; set; }

        public long? EncodedLength { get; set; }

        public TimeSpan? EncodeTime { get; set; }

        [JsonIgnore]
        public ICollection<RoomVideo>? PlaylistVideos { get; set; }
    }

    public enum VideoStatus
    {
        Uploading,
        Uploaded,
        Queued,
        Encoding,
        Ready,
        Cancelled,
        Failed,
        Deleted,
    }
}
