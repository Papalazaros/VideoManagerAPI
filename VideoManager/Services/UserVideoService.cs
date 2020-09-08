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
    public interface IUserVideoService
    {
        Task<List<Video>> GetAll(int? userId, int? roomId, VideoStatus? videoStatus);
        Task<Video> Get(int videoId);
        Task<Video> Create(IFormFile formFile);
        Task<List<Video>> CreateMany(IEnumerable<IFormFile> formFiles);
        Task<Video> Delete(int? userId, int videoId);
        Task<Video> FindByOriginalVideoName(int? userId, string originalVideoName);
        Task<List<Video>> GetRandom(int? userId, int roomId, VideoStatus? videoStatus, int count = 1);
    }

    public class UserVideoService : IUserVideoService
    {
        private readonly ILogger<UserVideoService> _logger;
        private readonly VideoManagerDbContext _videoManagerDbContext;
        private readonly IFileService _fileService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserVideoService(ILogger<UserVideoService> logger,
            VideoManagerDbContext videoManagerDbContext,
            IFileService fileService,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _videoManagerDbContext = videoManagerDbContext;
            _fileService = fileService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<Video>> GetRandom(int? userId, int roomId, VideoStatus? videoStatus, int count = 1)
        {
            List<Video> availableVideos = await GetAll(userId, roomId, videoStatus);

            List<Video> videos = new List<Video>(count);

            if (availableVideos.Count == 0 || count == 0) return videos;
            if (availableVideos.Count <= count) return availableVideos;

            Random random = new Random();

            for (int i = 0; i < count; i++)
            {
                videos.Add(availableVideos[random.Next(0, availableVideos.Count - 1)]);
            }

            return videos;
        }

        public async Task<Video> Get(int videoId)
        {
            return await _videoManagerDbContext.Videos
                .FirstOrDefaultAsync(x => x.VideoId == videoId);
        }

        public async Task<List<Video>> GetAll(int? userId, int? roomId, VideoStatus? videoStatus)
        {
            if (roomId.HasValue)
            {
                return await _videoManagerDbContext.RoomVideos
                    .AsNoTracking()
                    .Where(x => x.RoomId == roomId)
                    .Select(x => x.Video)
                    .Where(x => (!userId.HasValue || x.CreatedByUserId == userId) && (!videoStatus.HasValue || x.Status == videoStatus))
                    .ToListAsync();
            }
            else
            {
                return await _videoManagerDbContext.Videos
                    .AsNoTracking()
                    .Where(x => (!userId.HasValue || x.CreatedByUserId == userId) && (!videoStatus.HasValue || x.Status == videoStatus))
                    .ToListAsync();
            }
        }

        public async Task<Video> Delete(int? userId, int videoId)
        {
            if (!userId.HasValue) return null;

            Video video = await _videoManagerDbContext.Videos
                .FirstOrDefaultAsync(x => x.VideoId == videoId && x.CreatedByUserId == userId);

            if (video != null)
            {
                _videoManagerDbContext.Remove(video);
                await _videoManagerDbContext.SaveChangesAsync();
            }

            return video;
        }

        public async Task<Video> FindByOriginalVideoName(int? userId, string originalVideoName)
        {
            if (!userId.HasValue) return null;

            return await _videoManagerDbContext.Videos
                .FirstOrDefaultAsync(x => x.OriginalFileName == originalVideoName && x.CreatedByUserId == userId);
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

            await _videoManagerDbContext.Videos.AddRangeAsync(videos);
            await _videoManagerDbContext.SaveChangesAsync();
            await Task.WhenAll(fileCreationTasks);

            return videos;
        }

        public async Task<Video> Create(IFormFile formFile)
        {
            Video video = CreateVideoFromIFormFile(formFile);

            await _videoManagerDbContext.Videos.AddAsync(video);
            await _videoManagerDbContext.SaveChangesAsync();
            await _fileService.Create(formFile.OpenReadStream(), video.GetOriginalFilePath());

            return video;
        }

        private Video CreateVideoFromIFormFile(IFormFile formFile)
        {
            string fileExtension = Path.GetExtension(formFile.FileName);

            return new Video
            {
                OriginalFileName = formFile.FileName,
                OriginalLength = formFile.Length,
                OriginalType = fileExtension,
                Status = VideoStatus.Uploaded
            };
        }
    }
}
