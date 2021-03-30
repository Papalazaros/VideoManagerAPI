using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

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

            int thumbnailsAssigned = await videoService.AssignThumbnails();
            int previewsAssigned = await videoService.AssignPreviews();
            int durationsAssigned = await videoService.AssignDurations();

            return thumbnailsAssigned > 0 || previewsAssigned > 0 || durationsAssigned > 0;
        }
    }
}
