using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VideoManager.Models
{
    public class Playlist : BaseEntity
    {
        [Key]
        public int PlaylistId { get; set; }

        public string Name { get; set; }

        public ICollection<PlaylistVideo> PlaylistVideos { get; set; }
    }
}
