using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VideoManager.Models;

namespace VideoManager.Services
{
    public class EncodingService : BackgroundService
    {
        private readonly ILogger<EncodingService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private const int _defaultPollingInterval = 15;
        private const int _maxPollingInterval = 300;
        private static int _pollingInterval = _defaultPollingInterval;
        private const int _pollingIntervalIncreaseRate = 2;
        private const int _maxConcurrentTasks = 4;

        public EncodingService(ILogger<EncodingService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
               await DoWork();
            }
        }

        private async Task DoWork()
        {
            _logger.LogInformation(
                "EncodingService is working.");

            using IServiceScope scope = _scopeFactory.CreateScope();

            IVideoService videoService = scope.ServiceProvider.GetRequiredService<IVideoService>();

            List<Video> videos = await videoService.GetVideosToEncode(_maxConcurrentTasks);

            if (videos.Count == 0)
            {
                TimeSpan _timeToCheckInterval = TimeSpan.FromSeconds(_pollingInterval);

                _logger.LogInformation(
                    "No videos to encode. Sleeping for {_pollingInterval} seconds.", _timeToCheckInterval.TotalSeconds);

                await Task.Delay(_timeToCheckInterval);

                _pollingInterval = Math.Min(_maxPollingInterval, _pollingInterval * _pollingIntervalIncreaseRate);
            }
            else
            {
                VideoManagerDbContext videoManagerDbContext = scope.ServiceProvider.GetRequiredService<VideoManagerDbContext>();
                IEncoder encodingService = scope.ServiceProvider.GetRequiredService<IEncoder>();

                _pollingInterval = _defaultPollingInterval;

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
            }
        }
    }
}
