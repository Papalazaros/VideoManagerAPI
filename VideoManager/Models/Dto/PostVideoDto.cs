namespace VideoManager.Models.Dto
{
    public class PostVideoDto
    {
        public int? VideoId { get; set; }
        public string? OriginalFileName { get; set; }
        public VideoStatus? Status { get; set; }
    }
}
