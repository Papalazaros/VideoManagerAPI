using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VideoManager.Models
{
    public class Room : BaseEntity
    {
        [Key]
        public int RoomId { get; set; }

        [Required]
        public string Name { get; set; }

        public bool IsPrivate { get; set; } = true;

        public ICollection<RoomVideo> RoomVideos { get; set; }
    }
}
