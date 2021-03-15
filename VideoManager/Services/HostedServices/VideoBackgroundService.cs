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
        public VideoBackgroundService(ILogger<VideoBackgroundService> logger, IServiceScopeFactory scopeFactory)
            : base(logger, scopeFactory) { }

        public override async Task<bool> DoWork()
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            IVideoManagerService videoService = scope.ServiceProvider.GetRequiredService<IVideoManagerService>();

            IEnumerable<Video> orphanedFiles = await videoService.DeleteOrphaned();
            IEnumerable<Video> failedFiles = await videoService.DeleteFailed();
            int thumbnailsAssigned = await videoService.AssignThumbnails();
            int previewsAssigned = await videoService.AssignPreviews();
            int durationsAssigned = await videoService.AssignDurations();

            return orphanedFiles.Any() || failedFiles.Any() || thumbnailsAssigned > 0 || previewsAssigned > 0 || durationsAssigned > 0;
        }
    }
}
