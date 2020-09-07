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
        public int PlaylistId { get; set; }

        public string Name { get; set; }

        public ICollection<PlaylistVideo> PlaylistVideos { get; set; }
    }
}
