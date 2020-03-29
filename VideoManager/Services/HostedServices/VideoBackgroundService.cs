using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoManager.Models;

namespace VideoManager.Services
{
    public class VideoBackgroundService : BackgroundTaskService
    {
        public VideoBackgroundService(ILogger<VideoBackgroundService> logger, IServiceScopeFactory scopeFactory) : base(logger, scopeFactory) { }

        public override async Task<bool> DoWork()
        {
            _logger.LogInformation(
                "VideoBackgroundService is working.");

            using IServiceScope scope = _scopeFactory.CreateScope();
            IVideoService videoService = scope.ServiceProvider.GetRequiredService<IVideoService>();

            IEnumerable<Video> orphanedFiles = await videoService.DeleteOrphaned();
            IEnumerable<Video> failedFiles = await videoService.DeleteFailed();
            IEnumerable<Video> thumbnailsAssigned = await videoService.AssignThumbnails();
            IEnumerable<Video> durationsAssigned = await videoService.AssignDurations();

            return orphanedFiles.Any() || failedFiles.Any() || thumbnailsAssigned.Any() || durationsAssigned.Any();
        }
    }
}
