namespace VideoManager.Models
{
    public class RoomVideo
    {
        public int RoomId { get; set; }

        public Room? Room { get; set; }

        public int VideoId { get; set; }

        public Video? Video { get; set; }

        public RoomVideo(int roomId, int videoId)
        {
            RoomId = roomId;
            VideoId = videoId;
        }
    }
}
