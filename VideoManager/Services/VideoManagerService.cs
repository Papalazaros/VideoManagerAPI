using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VideoManager.Models;

namespace VideoManager.Services
{
    public interface IVideoManagerService
    {
        Task<List<Video>> GetVideosToEncode(int count);
        Task<IEnumerable<Video>> DeleteFailed();
        Task<IEnumerable<Video>> DeleteOrphaned();
        Task<IEnumerable<Video>> AssignThumbnails();
        Task<IEnumerable<Video>> AssignDurations();
    }

    public class VideoManagerService : IVideoManagerService
    {
        private readonly VideoManagerDbContext _videoManagerDbContext;
        private readonly IEncoder _encoderService;
        private static readonly DateTime _encodingCooldown = DateTime.UtcNow.AddMinutes(-30);

        public VideoManagerService(VideoManagerDbContext videoManagerDbContext,
            IEncoder encodingService)
        {
            _videoManagerDbContext = videoManagerDbContext;
            _encoderService = encodingService;
        }

        public async Task<IEnumerable<Video>> AssignDurations()
        {
            List<Video> videos = await _videoManagerDbContext.Videos
                .Where(x => x.Status == VideoStatus.Ready && !x.DurationInSeconds.HasValue)
                .ToListAsync();

            List<Task<int?>> videoDurationTasks = videos
                .Select(x => _encoderService.GetVideoDurationInSeconds(x.GetEncodedFilePath()))
                .ToList();

            for (int i = 0; i < videoDurationTasks.Count; i++)
            {
                int? taskResult = await videoDurationTasks[i];

                videos[i].DurationInSeconds = taskResult ?? 0;
            }

            await _videoManagerDbContext.SaveChangesAsync();

            return videos;
        }

        public async Task<IEnumerable<Video>> AssignThumbnails()
        {
            List<Video> videos = await _videoManagerDbContext.Videos
                .Where(x => x.Status == VideoStatus.Ready && string.IsNullOrEmpty(x.ThumbnailFilePath))
                .ToListAsync();

            List<Task<string?>> videoThumbnailTasks = videos
                .Select(x => _encoderService.CreateThumbnail(x))
                .ToList();

            for (int i = 0; i < videoThumbnailTasks.Count; i++)
            {
                videos[i].ThumbnailFilePath = await videoThumbnailTasks[i];
            }

            await _videoManagerDbContext.SaveChangesAsync();

            return videos;
        }

        public async Task<IEnumerable<Video>> DeleteFailed()
        {
            List<int> videoIdsToRemove = await _videoManagerDbContext.Videos
                    .Where(x => x.Status == VideoStatus.Failed)
                    .Select(x => x.VideoId)
                    .ToListAsync();

            return await DeleteMany(videoIdsToRemove);
        }

        public async Task<IEnumerable<Video>> DeleteOrphaned()
        {
            List<int> videoIdsToRemove =
                (
                await _videoManagerDbContext.Videos
                    .Where(x => x.Status == VideoStatus.Ready)
                    .Select(x => new { x.VideoId, EncodedFilePath = x.GetEncodedFilePath() })
                    .ToListAsync()
                )
                .Where(x => !File.Exists(x.EncodedFilePath))
                .Select(x => x.VideoId)
                .ToList();

            return await DeleteMany(videoIdsToRemove);
        }

        public Task<List<Video>> GetVideosToEncode(int count)
        {
            return _videoManagerDbContext.Videos
                .AsNoTracking()
                .Where(x => x.Status == VideoStatus.Uploaded
                    || (x.Status == VideoStatus.Encoding && x.CreatedDate < _encodingCooldown))
                .Take(count)
                .ToListAsync();
        }

        private async Task<IEnumerable<Video>> DeleteMany(IEnumerable<int> videoIds)
        {
            List<Video> videosToDelete = new();
            List<int> deletedVideoIds = new();

            foreach (int videoId in videoIds)
            {
                Video? video = await _videoManagerDbContext.Videos
                    .FirstOrDefaultAsync(x => x.VideoId == videoId);

                if (video != null)
                {
                    videosToDelete.Add(video);
                    deletedVideoIds.Add(video.VideoId);
                }
            }

            if (videosToDelete.Count > 0)
            {
                _videoManagerDbContext.RemoveRange(videosToDelete);
                await _videoManagerDbContext.SaveChangesAsync();
            }

            return videosToDelete;
        }
    }
}
