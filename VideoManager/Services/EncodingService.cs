using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using VideoManager.Models;

namespace VideoManager.Services
{
    public class EncodingService : BackgroundService
    {
        private readonly ILogger<EncodingService> _logger;
        private readonly IEncoder _encoder;
        private readonly IServiceScopeFactory _scopeFactory;

        private const int _pollingInterval = 15;

        private static readonly TimeSpan _timeToCheckInterval = TimeSpan.FromSeconds(_pollingInterval);

        public EncodingService(ILogger<EncodingService> logger, IServiceScopeFactory scopeFactory, IEncoder encoder)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _encoder = encoder;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            //Do your preparation (e.g. Start code) here
            while (!cancellationToken.IsCancellationRequested)
            {
               await DoWork();
            }

            //Do your cleanup (e.g. Stop code) here
        }

        private async Task DoWork()
        {
            _logger.LogInformation(
                "EncodingService is working.");

            using IServiceScope scope = _scopeFactory.CreateScope();

            IVideoService videoService = scope.ServiceProvider.GetRequiredService<IVideoService>();
            VideoManagerDbContext videoManagerDbContext = scope.ServiceProvider.GetRequiredService<VideoManagerDbContext>();

            Video video = await videoService.GetVideoToEncode();

            if (video == null)
            {
                _logger.LogInformation(
                    "No videos to encode. Sleeping for {_pollingInterval} seconds.", _timeToCheckInterval.TotalSeconds);

                await Task.Delay(_timeToCheckInterval);
            }
            else
            {
                video.Status = VideoStatus.Encoding;
                await videoManagerDbContext.SaveChangesAsync();

                _logger.LogInformation(
                    "Starting encode of {video}.", video);

                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                bool encodedSuccessfully = await _encoder.Encode(video);

                stopWatch.Stop();

                video.Status = encodedSuccessfully
                    ? VideoStatus.Ready
                    : VideoStatus.Failed;

                await videoManagerDbContext.SaveChangesAsync();

                _logger.LogInformation(
                    "Finished encode of {video} in {elapsedMilliseconds} milliseconds.", video, stopWatch.ElapsedMilliseconds);
            }
        }
    }
}
