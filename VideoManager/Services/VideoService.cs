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
        Task<List<Video>> GetAll(VideoStatus videoStatus);
        Task<List<Video>> GetAll();
        Task<Video> Get(Guid videoId);
        Task<List<Guid>> GetRandom(int count);
        Task<Video> GetVideoToEncode();
        Task<Video> Create(IFormFile formFile);
        Task<List<Video>> CreateMany(IEnumerable<IFormFile> formFiles);
        Task<List<Video>> DeleteFailed();
        Task<Video> Delete(Guid videoId);
    }

    public class VideoService : IVideoService
    {
        private readonly ILogger<VideoService> _logger;
        private readonly VideoManagerDbContext _videoManagerDbContext;
        private readonly IFileService _fileService;

        public VideoService(ILogger<VideoService> logger,
            VideoManagerDbContext videoManagerDbContext,
            IFileService fileService)
        {
            _logger = logger;
            _videoManagerDbContext = videoManagerDbContext;
            _fileService = fileService;
        }

        public async Task<Video> GetVideoToEncode()
        {
            DateTime encodingCooldown = DateTime.UtcNow.AddMinutes(-30);

            return await _videoManagerDbContext.Videos
                .FirstOrDefaultAsync(x => x.Status == VideoStatus.Uploaded
                    || (x.Status == VideoStatus.Encoding && x.CreatedDate < encodingCooldown));
        }

        public async Task<List<Guid>> GetRandom(int count)
        {
            List<Guid> availableVideoIds = await _videoManagerDbContext.Videos
                .AsNoTracking()
                .Where(x => x.Status == VideoStatus.Ready)
                .Select(x => x.Id)
                .ToListAsync();

            List<Guid> guids = new List<Guid>(count);

            if (availableVideoIds.Count == 0)
            {
                return guids;
            }

            Random random = new Random();

            for(int i = 0; i < count; i++)
            {
                guids.Add(availableVideoIds[random.Next(0, availableVideoIds.Count - 1)]);
            }

            return guids;
        }

        public async Task<Video> Get(Guid videoId)
        {
            return await _videoManagerDbContext.Videos.FindAsync(videoId);
        }

        public async Task<Video> Delete(Guid videoId)
        {
            Video video = await _videoManagerDbContext.Videos.FindAsync(videoId);

            _videoManagerDbContext.Remove(video);

            await _videoManagerDbContext.SaveChangesAsync();

            return video;
        }

        public async Task<List<Video>> DeleteFailed()
        {
            List<Video> failedVideos = await _videoManagerDbContext.Videos
                .AsNoTracking()
                .Where(x => x.Status == VideoStatus.Failed)
                .ToListAsync();

            _videoManagerDbContext.RemoveRange(failedVideos);

            await _videoManagerDbContext.SaveChangesAsync();

            return failedVideos;
        }

        public async Task<List<Video>> CreateMany(IEnumerable<IFormFile> formFiles)
        {
            List<Task> fileCreationTasks = new List<Task>();
            List<Video> videos = new List<Video>();

            foreach (IFormFile formFile in formFiles)
            {
                Guid guid = Guid.NewGuid();
                string fileExtension = Path.GetExtension(formFile.FileName);

                Video video = new Video
                {
                    Id = guid,
                    AssignedName = $@"Videos\{guid}{fileExtension}",
                    UserProvidedName = formFile.FileName,
                    Length = formFile.Length,
                    Type = fileExtension,
                    Status = VideoStatus.Uploaded
                };

                videos.Add(video);
                fileCreationTasks.Add(_fileService.Create(formFile.OpenReadStream(), video.AssignedName));
            }

            await Task.WhenAll(fileCreationTasks);
            await _videoManagerDbContext.Videos.AddRangeAsync(videos);
            await _videoManagerDbContext.SaveChangesAsync();

            return videos;
        }

        public async Task<Video> Create(IFormFile formFile)
        {
            Guid guid = Guid.NewGuid();
            string fileExtension = Path.GetExtension(formFile.FileName);

            Video video = new Video
            {
                Id = guid,
                AssignedName = $@"Videos\{guid}{fileExtension}",
                UserProvidedName = formFile.FileName,
                Length = formFile.Length,
                Type = fileExtension,
                Status = VideoStatus.Uploaded
            };

            await _fileService.Create(formFile.OpenReadStream(), video.AssignedName);
            await _videoManagerDbContext.Videos.AddAsync(video);
            await _videoManagerDbContext.SaveChangesAsync();

            return video;
        }

        public async Task<List<Video>> GetAll(VideoStatus videoStatus)
        {
            return await _videoManagerDbContext.Videos
                .AsNoTracking()
                .Where(x => x.Status == videoStatus)
                .ToListAsync();
        }

        public async Task<List<Video>> GetAll()
        {
            return await _videoManagerDbContext.Videos
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
