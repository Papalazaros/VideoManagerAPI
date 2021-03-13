using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VideoManager.Exceptions;
using VideoManager.Models;

namespace VideoManager.Services
{
    public interface IUserVideoService
    {
        Task<List<Video>> GetAll(int? userId, int? roomId, VideoStatus? videoStatus);
        Task<Video> Get(int videoId);
        Task<Video> Create(IFormFile formFile);
        Task<List<Video>> CreateMany(IEnumerable<IFormFile> formFiles);
        Task<Video?> Delete(int? userId, int videoId);
        Task<Video?> FindByOriginalVideoName(int? userId, string originalVideoName);
    }

    public class UserVideoService : IUserVideoService
    {
        private readonly VideoManagerDbContext _videoManagerDbContext;
        private readonly IFileService _fileService;

        public UserVideoService(VideoManagerDbContext videoManagerDbContext,
            IFileService fileService)
        {
            _videoManagerDbContext = videoManagerDbContext;
            _fileService = fileService;
        }

        public async Task<Video> Get(int videoId)
        {
            Video video = await _videoManagerDbContext.Videos.FindAsync(videoId);
            if (video == null || video.Status == VideoStatus.Deleted) throw new NotFoundException();
            return video;
        }

        public Task<List<Video>> GetAll(int? userId, int? roomId, VideoStatus? videoStatus)
        {
            if (roomId.HasValue)
            {
                return _videoManagerDbContext.RoomVideos
                    .AsNoTracking()
                    .Include(x => x.Video)
                    .Where(x => x.RoomId == roomId && x.Video != null && (!videoStatus.HasValue || x.Video.Status == videoStatus))
                    .Select(x => x.Video!)
                    .ToListAsync();
            }
            else
            {
                return _videoManagerDbContext.Videos
                    .AsNoTracking()
                    .Where(x => (!userId.HasValue || x.CreatedByUserId == userId) && (!videoStatus.HasValue || x.Status == videoStatus))
                    .ToListAsync();
            }
        }

        public async Task<Video?> Delete(int? userId, int videoId)
        {
            Video video = await Get(videoId);

            if (video.CreatedByUserId != userId) throw new UnauthorizedAccessException();

            video.Status = VideoStatus.Deleted;
            await _videoManagerDbContext.SaveChangesAsync();

            return video;
        }

        public Task<Video?> FindByOriginalVideoName(int? userId, string originalVideoName)
        {
            return _videoManagerDbContext.Videos
                .FirstOrDefaultAsync(x => x.OriginalFileName == originalVideoName && x.CreatedByUserId == userId)!;
        }

        public async Task<List<Video>> CreateMany(IEnumerable<IFormFile> formFiles)
        {
            List<Task> fileCreationTasks = new();
            List<Video> videos = new();

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

        private static Video CreateVideoFromIFormFile(IFormFile formFile)
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
