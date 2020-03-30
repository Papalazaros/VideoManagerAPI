using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VideoManager.Models
{
    public class VideoDto
    {
        public Guid VideoId { get; set; }
        public string OriginalFileName { get; set; }
        public long? Length { get; set; }
        public int? DurationInSeconds { get; set; }
    }
}
