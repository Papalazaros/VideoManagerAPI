namespace VideoManager.Models
{
    public enum VideoStatus
    {
        Uploading,
        Uploaded,
        Queued,
        Encoding,
        Ready,
        Cancelled,
        Failed,
        Deleted,
    }
}
