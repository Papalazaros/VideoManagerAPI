using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VideoManager.Models.Dto
{
    public class GetVideoDto
    {
        public int VideoId { get; set; }
        public string OriginalFileName { get; set; }
        public long? Length { get; set; }
        public int? DurationInSeconds { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
    }
}
