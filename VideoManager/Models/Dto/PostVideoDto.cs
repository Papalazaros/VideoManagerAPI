namespace VideoManager.Models.DTO
{
    public class PostVideoDTO
    {
        public int? VideoId { get; set; }
        public string? OriginalFileName { get; set; }
        public VideoStatus? Status { get; set; }
    }
}
