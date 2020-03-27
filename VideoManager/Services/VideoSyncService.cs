using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VideoManager.Models;

namespace VideoManager.Services
{
    public interface IVideoSyncService
    {
        Task SyncVideoAsync(Guid roomId, VideoSyncMessage videoSyncMessage);
    }

    public class VideoSyncService : IVideoSyncService
    {
        private readonly IHubContext<VideoHub> _videoHub;
        private readonly ILogger<VideoService> logger;

        public VideoSyncService(ILogger<VideoService> logger,
            IHubContext<VideoHub> videoHub)
        {
            _videoHub = videoHub;
        }

        public async Task SyncVideoAsync(Guid roomId, VideoSyncMessage videoSyncMessage)
        {
            await _videoHub.Clients.Group(roomId.ToString()).SendAsync("VideoSyncMessage", videoSyncMessage);
        }
    }
}
