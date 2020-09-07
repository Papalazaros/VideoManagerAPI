using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using VideoManager.Models;

namespace VideoManager.Services
{
    public class EncodingService : BackgroundTaskService
    {
        private const int _maxConcurrentTasks = 4;

        public EncodingService(ILogger<EncodingService> logger, IServiceScopeFactory scopeFactory) : base(logger, scopeFactory) { }

        public override async Task<bool> DoWork()
        {
            _logger.LogInformation(
                "EncodingService is working.");

            using IServiceScope scope = _scopeFactory.CreateScope();
            IVideoManagerService videoService = scope.ServiceProvider.GetRequiredService<IVideoManagerService>();
            List<Video> videos = await videoService.GetVideosToEncode(_maxConcurrentTasks);

            if (videos.Count == 0) return false;

            VideoManagerDbContext videoManagerDbContext = scope.ServiceProvider.GetRequiredService<VideoManagerDbContext>();
            IEncoder encodingService = scope.ServiceProvider.GetRequiredService<IEncoder>();
            List<Task<EncodeResult>> encodingTasks = new List<Task<EncodeResult>>(videos.Count);

            videoManagerDbContext.UpdateRange(videos);

            foreach (Video video in videos)
            {
                encodingTasks.Add(encodingService.Encode(video));
                video.Status = VideoStatus.Encoding;
            }

            await videoManagerDbContext.SaveChangesAsync();

            for (int i = 0; i < encodingTasks.Count; i++)
            {
                Video video = videos[i];

                EncodeResult encodeResult = await encodingTasks[i];

                video.Status = encodeResult.Success
                    ? VideoStatus.Ready
                    : VideoStatus.Failed;

                video.EncodeTime = encodeResult.EncodeTime;
                video.EncodedLength = encodeResult.EncodedFileLength;
            }

            await videoManagerDbContext.SaveChangesAsync();

            return true;
        }
    }
}
