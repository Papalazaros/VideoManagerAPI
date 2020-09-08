namespace VideoManager.Models
{
    public class RoomMember
    {
        public int RoomId { get; set; }

        public Room Room { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }
    }
}
