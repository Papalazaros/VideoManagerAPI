﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VideoManager.Models;

namespace VideoManager.Services
{
    public class EncodingBackgroundService : BackgroundTaskService
    {
        private static readonly int _maxConcurrentTasks = Environment.ProcessorCount > 1 ? Environment.ProcessorCount - 1 : 1;

        public EncodingBackgroundService(ILogger<EncodingBackgroundService> logger, IServiceScopeFactory scopeFactory) : base(logger, scopeFactory) { }

        public override async Task<bool> DoWork()
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            IVideoManagerService videoService = scope.ServiceProvider.GetRequiredService<IVideoManagerService>();
            List<Video> videos = await videoService.GetVideosToEncode(_maxConcurrentTasks);

            if (videos.Count == 0)
            {
                return false;
            }

            VideoManagerDbContext videoManagerDbContext = scope.ServiceProvider.GetRequiredService<VideoManagerDbContext>();
            IEncoder encodingService = scope.ServiceProvider.GetRequiredService<IEncoder>();
            List<Task<EncodeResult>> encodingTasks = new(videos.Count);

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
