using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VideoManager.Models
{
    public class RoomVideo
    {
        public Guid RoomId { get; set; }
        public Room Room { get; set; }
        public Guid VideoId { get; set; }
        public Video Video { get; set; }
    }
}
