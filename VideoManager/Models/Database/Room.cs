using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VideoManager.Models.Database;

namespace VideoManager.Models
{
    public class Room : BaseEntity
    {
        [Key]
        public int RoomId { get; set; }

        public string Name { get; set; }

        public bool IsPrivate { get; set; } = true;

        public ICollection<RoomVideo>? RoomVideos { get; set; }

        public RoomStatus RoomStatus { get; set; } = RoomStatus.Active;

        public Room(string name)
        {
            Name = name;
        }
    }
}
