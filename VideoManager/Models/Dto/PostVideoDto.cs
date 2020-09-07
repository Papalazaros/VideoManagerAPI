using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VideoManager.Models.Dto
{
    public class PostVideoDto
    {
        public int VideoId { get; set; }
        public string OriginalFileName { get; set; }
        public VideoStatus Status { get; set; }
    }
}
