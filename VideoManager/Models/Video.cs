using System;
using System.ComponentModel.DataAnnotations;

namespace VideoManager.Models
{
    public class Video
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string AssignedName { get; set; }

        [Required]
        public string UserProvidedName { get; set; }

        [Required]
        public VideoStatus Status { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public long Length { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public override string ToString()
        {
            return $@"Guid: {Id}
                     AssignedName: {AssignedName}
                     UserProvidedName: {UserProvidedName}
                     VideoStatus: {Status}
                     Type: {Type}
                     Length: {Length}";
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
