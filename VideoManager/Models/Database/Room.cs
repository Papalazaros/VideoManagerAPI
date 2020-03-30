using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VideoManager.Models
{
    public class Room : BaseEntity
    {
        [Key]
        public Guid RoomId { get; set; }

        [Required]
        public string Name { get; set; }
        public Guid? OwnerId { get; set; }
        public User Owner { get; set; }
        public bool IsPrivate { get; set; } = true;
        public Guid PlaylistId { get; set; }
        public Playlist Playlist { get; set; }
    }
}
