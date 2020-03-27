using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VideoManager.Models;

namespace VideoManager.Services
{
    public interface IVideoService
    {
        Task<List<Video>> GetAll(VideoStatus? videoStatus = null);
        Task<List<Guid>> GetAllIds(VideoStatus? videoStatus = null);
        Task<Video> Get(Guid videoId);
        Task<List<Video>> GetRandom(int count);
        Task<List<Video>> GetVideosToEncode(int count);
        Task<Video> Create(IFormFile formFile);
        Task<List<Video>> CreateMany(IEnumerable<IFormFile> formFiles);
        Task<IEnumerable<Video>> DeleteMany(IEnumerable<Guid> videoIds);
        Task<Video> Delete(Guid videoId);
        Task<IEnumerable<Video>> DeleteFailed();
        Task<IEnumerable<Video>> DeleteOrphaned();
        Task<IEnumerable<Video>> AssignThumbnails();
        Task<IEnumerable<Video>> AssignDurations();
    }

    public class VideoService : IVideoService
    {
        private readonly ILogger<VideoService> _logger;
        private readonly VideoManagerDbContext _videoManagerDbContext;
        private readonly IFileService _fileService;
        private readonly IEncoder _encodingService;
        private static readonly DateTime _encodingCooldown = DateTime.UtcNow.AddMinutes(-30);

        public VideoService(ILogger<VideoService> logger,
            VideoManagerDbContext videoManagerDbContext,
            IFileService fileService,
            IEncoder encodingService)
        {
            _logger = logger;
            _videoManagerDbContext = videoManagerDbContext;
            _fileService = fileService;
            _encodingService = encodingService;
        }

        public async Task<IEnumerable<Video>> AssignDurations()
        {
            List<Video> videos = await _videoManagerDbContext.Videos
                .Where(x => x.Status == VideoStatus.Ready && !x.DurationInSeconds.HasValue)
                .ToListAsync();

            List<Task<int?>> videoDurationTasks = videos
                .Select(x => _encodingService.GetVideoDurationInSeconds(x.GetEncodedFilePath()))
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

            List<Task<string>> videoThumbnailTasks = videos
                .Select(x => _encodingService.CreateThumbnail(x))
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
            List<Guid> videoIdsToRemove = await _videoManagerDbContext.Videos
                    .Where(x => x.Status == VideoStatus.Failed)
                    .Select(x => x.Id)
                    .ToListAsync();

            return await DeleteMany(videoIdsToRemove);
        }

        public async Task<IEnumerable<Video>> DeleteOrphaned()
        {
            List<Guid> videoIdsToRemove =
                (
                await _videoManagerDbContext.Videos
                    .Where(x => x.Status == VideoStatus.Ready)
                    .Select(x => new { x.Id, EncodedFilePath = x.GetEncodedFilePath() })
                    .ToListAsync()
                )
                .Where(x => !File.Exists(x.EncodedFilePath))
                .Select(x => x.Id)
                .ToList();

            return await DeleteMany(videoIdsToRemove);
        }

        public async Task<List<Video>> GetVideosToEncode(int count)
        {
            return await _videoManagerDbContext.Videos
                .AsNoTracking()
                .Where(x => x.Status == VideoStatus.Uploaded
                    || (x.Status == VideoStatus.Encoding && x.CreatedDate < _encodingCooldown))
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Video>> GetRandom(int count)
        {
            List<Video> availableVideoIds = await _videoManagerDbContext.Videos
                .AsNoTracking()
                .Where(x => x.Status == VideoStatus.Ready)
                .ToListAsync();

            List<Video> videos = new List<Video>(count);

            if (availableVideoIds.Count == 0) return videos;

            Random random = new Random();

            for(int i = 0; i < count; i++)
            {
                videos.Add(availableVideoIds[random.Next(0, availableVideoIds.Count - 1)]);
            }

            return videos;
        }

        public async Task<Video> Get(Guid videoId)
        {
            return await _videoManagerDbContext.Videos.FindAsync(videoId);
        }

        public async Task<Video> Delete(Guid videoId)
        {
            Video video = await _videoManagerDbContext.Videos.FindAsync(videoId);

            if (video != null)
            {
                _videoManagerDbContext.Remove(video);
                await _videoManagerDbContext.SaveChangesAsync();
            }

            return video;
        }

        public async Task<IEnumerable<Video>> DeleteMany(IEnumerable<Guid> videoIds)
        {
            List<Video> videosToDelete = new List<Video>();
            List<Guid> deletedVideoIds = new List<Guid>();

            foreach (Guid videoId in videoIds)
            {
                Video video = await _videoManagerDbContext.Videos.FindAsync(videoId);

                if (video != null)
                {
                    videosToDelete.Add(video);
                    deletedVideoIds.Add(video.Id);
                }
            }

            if (videosToDelete.Count > 0)
            {
                _videoManagerDbContext.RemoveRange(videosToDelete);
                await _videoManagerDbContext.SaveChangesAsync();
            }

            return videosToDelete;
        }

        private Video CreateVideoFromIFormFile(IFormFile formFile)
        {
            Guid guid = Guid.NewGuid();
            string fileExtension = Path.GetExtension(formFile.FileName);

            return new Video
            {
                Id = guid,
                OriginalFileName = formFile.FileName,
                OriginalLength = formFile.Length,
                OriginalType = fileExtension,
                Status = VideoStatus.Uploaded
            };
        }

        public async Task<List<Video>> CreateMany(IEnumerable<IFormFile> formFiles)
        {
            List<Task> fileCreationTasks = new List<Task>();
            List<Video> videos = new List<Video>();

            foreach (IFormFile formFile in formFiles)
            {
                Video video = CreateVideoFromIFormFile(formFile);
                videos.Add(video);
                fileCreationTasks.Add(_fileService.Create(formFile.OpenReadStream(), video.GetOriginalFilePath()));
            }

            await Task.WhenAll(fileCreationTasks);
            await _videoManagerDbContext.Videos.AddRangeAsync(videos);
            await _videoManagerDbContext.SaveChangesAsync();

            return videos;
        }

        public async Task<Video> Create(IFormFile formFile)
        {
            Video video = CreateVideoFromIFormFile(formFile);

            await _fileService.Create(formFile.OpenReadStream(), video.GetOriginalFilePath());
            await _videoManagerDbContext.Videos.AddAsync(video);
            await _videoManagerDbContext.SaveChangesAsync();

            return video;
        }

        public async Task<List<Video>> GetAll(VideoStatus? videoStatus)
        {
            return await _videoManagerDbContext.Videos
                .AsNoTracking()
                .Where(x => !videoStatus.HasValue || x.Status == videoStatus)
                .ToListAsync();
        }

        public async Task<List<Guid>> GetAllIds(VideoStatus? videoStatus)
        {
            return await _videoManagerDbContext.Videos
                .AsNoTracking()
                .Where(x => !videoStatus.HasValue || x.Status == videoStatus)
                .Select(x => x.Id)
                .ToListAsync();
        }
    }
}
