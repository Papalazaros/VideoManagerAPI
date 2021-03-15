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
            var tasks = (await _videoManagerDbContext.Videos
                .Where(x => x.Status == VideoStatus.Ready && !x.DurationInSeconds.HasValue)
                .ToArrayAsync())
                .Select(x => new { video = x, task = _encoderService.GetVideoDurationInSeconds(x.GetEncodedFilePath()) })
                .ToArray();

            foreach (var taskObject in tasks)
            {
                taskObject.video.DurationInSeconds = await taskObject.task;
            }

            await _videoManagerDbContext.SaveChangesAsync();

            return tasks.Length;
        }

        public async Task<int> AssignThumbnails()
        {
            var tasks = (await _videoManagerDbContext.Videos
                .Where(x => x.Status == VideoStatus.Ready)
                .ToArrayAsync())
                .Where(x => string.IsNullOrEmpty(x.ThumbnailFilePath) || !File.Exists(x.ThumbnailFilePath))
                .Select(x => new { video = x, task = _encoderService.CreateThumbnail(x) })
                .ToArray();

            foreach (var taskObject in tasks)
            {
                taskObject.video.ThumbnailFilePath = await taskObject.task;
            }

            await _videoManagerDbContext.SaveChangesAsync();

            return tasks.Length;
        }

        public async Task<int> AssignPreviews()
        {
            var tasks = (await _videoManagerDbContext.Videos
                .Where(x => x.Status == VideoStatus.Ready)
                .ToArrayAsync())
                .Where(x => string.IsNullOrEmpty(x.PreviewFilePath) || !File.Exists(x.PreviewFilePath))
                .Select(x => new { video = x, task = _encoderService.CreatePreview(x) })
                .ToArray();

            foreach (var taskObject in tasks)
            {
                taskObject.video.PreviewFilePath = await taskObject.task;
            }

            await _videoManagerDbContext.SaveChangesAsync();

            return tasks.Length;
        }

        public Task<IEnumerable<Video>> DeleteFailed()
        {
            return DeleteMany(_videoManagerDbContext.Videos.Where(x => x.Status == VideoStatus.Failed));
        }

        public async Task<IEnumerable<Video>> DeleteOrphaned()
        {
            IEnumerable<Video> videosToRemove =
                (
                await _videoManagerDbContext.Videos
                    .Where(x => x.Status == VideoStatus.Ready)
                    .Select(x => new { video = x, encodedFilePath = x.GetEncodedFilePath() })
                    .ToListAsync()
                )
                .Where(x => !File.Exists(x.encodedFilePath))
                .Select(x => x.video);

            return await DeleteMany(videosToRemove);
        }

        public Task<List<Video>> GetVideosToEncode(int count)
        {
            return _videoManagerDbContext.Videos
                .Where(x => x.Status == VideoStatus.Uploaded
                    || (x.Status == VideoStatus.Encoding && x.CreatedDate < _encodingCooldown))
                .OrderBy(x => x.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        private async Task<IEnumerable<Video>> DeleteMany(IEnumerable<Video> videos)
        {
            _videoManagerDbContext.UpdateRange(videos);
            _videoManagerDbContext.RemoveRange(videos);
            await _videoManagerDbContext.SaveChangesAsync();
            return videos;
        }
    }
}
