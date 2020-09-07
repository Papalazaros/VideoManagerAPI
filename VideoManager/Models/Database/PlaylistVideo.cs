using System;

namespace VideoManager.Models
{
    public class PlaylistVideo
    {
        public int PlaylistId { get; set; }

        public Playlist Playlist { get; set; }

        public int VideoId { get; set; }

        public Video Video { get; set; }
    }
}
