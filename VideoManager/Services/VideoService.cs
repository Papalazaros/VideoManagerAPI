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
    }

    public class VideoService : IVideoService
    {
        private readonly ILogger<VideoService> _logger;
        private readonly VideoManagerDbContext _videoManagerDbContext;
        private readonly IFileService _fileService;
        private static readonly DateTime _encodingCooldown = DateTime.UtcNow.AddMinutes(-30);

        public VideoService(ILogger<VideoService> logger,
            VideoManagerDbContext videoManagerDbContext,
            IFileService fileService)
        {
            _logger = logger;
            _videoManagerDbContext = videoManagerDbContext;
            _fileService = fileService;
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
