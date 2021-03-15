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
        Task<int> AssignThumbnails();
        Task<int> AssignPreviews();
        Task<int> AssignDurations();
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

        public async Task<int> AssignDurations()
        {
            var videoTasks = (await _videoManagerDbContext.Videos
                .Where(x => x.Status == VideoStatus.Ready && !x.DurationInSeconds.HasValue)
                .ToArrayAsync())
                .Select(x => new { video = x, task = _encoderService.GetVideoDurationInSeconds(x.GetEncodedFilePath()) })
                .ToArray();

            foreach (var taskObject in videoTasks)
            {
                taskObject.video.DurationInSeconds = await taskObject.task;
            }

            await _videoManagerDbContext.SaveChangesAsync();

            return videoTasks.Length;
        }

        public async Task<int> AssignThumbnails()
        {
            var videoTasks = (await _videoManagerDbContext.Videos
                .Where(x => x.Status == VideoStatus.Ready)
                .ToArrayAsync())
                .Where(x => string.IsNullOrEmpty(x.ThumbnailFilePath) || !File.Exists(x.ThumbnailFilePath))
                .Select(x => new { video = x, task = _encoderService.CreateThumbnail(x) })
                .ToArray();

            foreach (var taskObject in videoTasks)
            {
                taskObject.video.ThumbnailFilePath = await taskObject.task;
            }

            await _videoManagerDbContext.SaveChangesAsync();

            return videoTasks.Length;
        }

        public async Task<int> AssignPreviews()
        {
            var videoTasks = (await _videoManagerDbContext.Videos
                .Where(x => x.Status == VideoStatus.Ready)
                .ToArrayAsync())
                .Where(x => string.IsNullOrEmpty(x.PreviewFilePath) || !File.Exists(x.PreviewFilePath))
                .Select(x => new { video = x, task = _encoderService.CreatePreview(x) })
                .ToArray();

            foreach(var taskObject in videoTasks)
            {
                taskObject.video.PreviewFilePath = await taskObject.task;
            }

            await _videoManagerDbContext.SaveChangesAsync();

            return videoTasks.Length;
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
                .OrderBy(x => x.CreatedDate)
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
