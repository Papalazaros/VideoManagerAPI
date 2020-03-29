using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VideoManager.Models
{
    public class Playlist : BaseEntity
    {
        [Key]
        public Guid PlaylistId { get; set; }
        public string Name => $"{RoomId}_{PlaylistId}";
        public Guid RoomId { get; set; }
        public Room Room { get; set; }

        public ICollection<PlaylistVideo> PlaylistVideos { get; set; }
    }
}
