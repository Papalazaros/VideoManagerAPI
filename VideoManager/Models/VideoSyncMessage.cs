namespace VideoManager.Models
{
    public class VideoSyncMessage
    {
        public VideoSyncOperation VideoSyncOperation { get; set; }
        public object Payload { get; set; }
    }

    public enum VideoSyncOperation
    {
        Stop,
        Pause,
        Play,
        Seek,
        ChangeVideo
    }
}
