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
        Task<List<Video>> GetAll(VideoStatus? videoStatus = null);
        Task<IEnumerable<Video>> GetAllByRoomId(int roomId);
        Task<Video> Get(int videoId);
        Task<Video> Create(IFormFile formFile);
        Task<List<Video>> CreateMany(IEnumerable<IFormFile> formFiles);
        Task<Video> Delete(int videoId);
        Task<Video> FindByOriginalVideoName(string originalVideoName);
    }

    public class UserVideoService : IUserVideoService
    {
        private readonly ILogger<UserVideoService> _logger;
        private readonly VideoManagerDbContext _videoManagerDbContext;
        private readonly IFileService _fileService;
        private readonly IEncoder _encodingService;
        private static readonly DateTime _encodingCooldown = DateTime.UtcNow.AddMinutes(-30);
        private readonly IHttpContextAccessor _httpContextAccessor;
        private int? UserId => (int?)_httpContextAccessor.HttpContext?.Items["UserId"];

        public UserVideoService(ILogger<UserVideoService> logger,
            VideoManagerDbContext videoManagerDbContext,
            IFileService fileService,
            IEncoder encodingService,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _videoManagerDbContext = videoManagerDbContext;
            _fileService = fileService;
            _encodingService = encodingService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<Video>> GetAllByRoomId(int roomId)
        {
            Room room = await _videoManagerDbContext.Rooms
                .Include(x => x.Playlist)
                .FirstOrDefaultAsync(x => x.RoomId == roomId);

            if (room == null) return Enumerable.Empty<Video>();

            return room.Playlist.PlaylistVideos.Select(x => x.Video);
        }

        //public async Task<List<Video>> GetRandom(int count)
        //{
        //    List<Video> availableVideoIds = await _videoManagerDbContext.Videos
        //        .AsNoTracking()
        //        .Where(x => x.Status == VideoStatus.Ready)
        //        .ToListAsync();

        //    List<Video> videos = new List<Video>(count);

        //    if (availableVideoIds.Count == 0) return videos;

        //    Random random = new Random();

        //    for(int i = 0; i < count; i++)
        //    {
        //        videos.Add(availableVideoIds[random.Next(0, availableVideoIds.Count - 1)]);
        //    }

        //    return videos;
        //}

        public async Task<Video> Get(int videoId)
        {
            return await _videoManagerDbContext.Videos
                .FirstOrDefaultAsync(x => x.VideoId == videoId && x.CreatedByUserId == UserId);
        }

        public async Task<Video> Delete(int videoId)
        {
            Video video = await _videoManagerDbContext.Videos
                .FirstOrDefaultAsync(x => x.VideoId == videoId && x.CreatedByUserId == UserId);

            if (video != null)
            {
                _videoManagerDbContext.Remove(video);
                await _videoManagerDbContext.SaveChangesAsync();
            }

            return video;
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

        public async Task<Video> FindByOriginalVideoName(string originalVideoName)
        {
            return await _videoManagerDbContext.Videos
                .FirstOrDefaultAsync(x => x.OriginalFileName == originalVideoName);
        }

        public async Task<Video> Create(IFormFile formFile)
        {
            Video video = CreateVideoFromIFormFile(formFile);

            Playlist playlist = await _videoManagerDbContext.Playlists.FirstOrDefaultAsync(x => x.Name == "DEFAULT");

            PlaylistVideo playlistVideo = new PlaylistVideo
            {
                VideoId = video.VideoId,
                PlaylistId = playlist.PlaylistId
            };

            await _fileService.Create(formFile.OpenReadStream(), video.GetOriginalFilePath());
            await _videoManagerDbContext.Videos.AddAsync(video);
            await _videoManagerDbContext.PlaylistVideos.AddAsync(playlistVideo);
            await _videoManagerDbContext.SaveChangesAsync();

            return video;
        }

        public async Task<List<Video>> GetAll(VideoStatus? videoStatus)
        {
            return await _videoManagerDbContext.Videos
                .AsNoTracking()
                .Where(x => x.CreatedByUserId == UserId && (!videoStatus.HasValue || x.Status == videoStatus))
                .ToListAsync();
        }

        private Video CreateVideoFromIFormFile(IFormFile formFile)
        {
            Guid guid = Guid.NewGuid();
            string fileExtension = Path.GetExtension(formFile.FileName);

            return new Video
            {
                OriginalFileName = formFile.FileName,
                OriginalLength = formFile.Length,
                OriginalType = fileExtension,
                Status = VideoStatus.Uploaded,
                IpAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString()
            };
        }
    }
}
