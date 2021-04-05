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
        Task<IReadOnlyCollection<Video>> GetAll(int? roomId, VideoStatus? videoStatus);
        Task<Video> Get(int videoId);
        Task<IEnumerable<Video>> Create(IEnumerable<IFormFile> formFiles);
        Task<Video?> Delete(int videoId);
        Task<Video?> FindByOriginalVideoName(string originalVideoName);
        Task<long> GetRemainingSpace();
    }

    public class UserVideoService : IUserVideoService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly VideoManagerDbContext _videoManagerDbContext;
        private readonly IFileService _fileService;
        private const long totalAvailableSpaceInBytes = 1000000000;
        private int? _userId => (int?)_httpContextAccessor.HttpContext?.Items["UserId"];

        public UserVideoService(VideoManagerDbContext videoManagerDbContext,
            IFileService fileService,
            IHttpContextAccessor httpContextAccessor)
        {
            _videoManagerDbContext = videoManagerDbContext;
            _fileService = fileService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Video> Get(int videoId)
        {
            Video video = await _videoManagerDbContext.Videos.FindAsync(videoId);
            if (video == null || video.Status == VideoStatus.Deleted)
            {
                throw new NotFoundException();
            }

            bool hasAccess = video.CreatedByUserId == _userId || _videoManagerDbContext.RoomMembers.Where(x => x.UserId == _userId).Join(_videoManagerDbContext.RoomVideos.Where(x => x.VideoId == videoId), x => x.RoomId, x => x.RoomId, (member, _) => member).Any();
            if (!hasAccess)
            {
                throw new UnauthorizedAccessException();
            }

            return video;
        }

        public async Task<IReadOnlyCollection<Video>> GetAll(int? roomId, VideoStatus? videoStatus)
        {
            if (roomId.HasValue)
            {
                return await _videoManagerDbContext.RoomVideos
                    .AsNoTracking()
                    .Include(x => x.Video)
                    .Where(x => x.RoomId == roomId && x.Video != null && (!videoStatus.HasValue || x.Video.Status == videoStatus))
                    .Select(x => x.Video!)
                    .ToArrayAsync();
            }
            else
            {
                return await _videoManagerDbContext.Videos
                    .AsNoTracking()
                    .Where(x => (!_userId.HasValue || x.CreatedByUserId == _userId) && (!videoStatus.HasValue || x.Status == videoStatus))
                    .ToArrayAsync();
            }
        }

        public async Task<Video?> Delete(int videoId)
        {
            Video video = await Get(videoId);

            video.Status = VideoStatus.Deleted;
            await _videoManagerDbContext.SaveChangesAsync();

            return video;
        }

        public Task<Video?> FindByOriginalVideoName(string originalVideoName)
        {
            return _videoManagerDbContext.Videos
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.OriginalFileName == originalVideoName && x.CreatedByUserId == _userId && x.Status != VideoStatus.Deleted)!;
        }

        public async Task<IEnumerable<Video>> Create(IEnumerable<IFormFile> formFiles)
        {
            List<Task> fileCreationTasks = new();
            List<Video> videos = new();

            foreach (IFormFile formFile in formFiles)
            {
                Video video = CreateVideoFromIFormFile(formFile);
                videos.Add(video);
                using Stream dataStream = formFile.OpenReadStream();
                fileCreationTasks.Add(_fileService.Create(dataStream, video.GetOriginalFilePath()));
            }

            await _videoManagerDbContext.Videos.AddRangeAsync(videos);
            await _videoManagerDbContext.SaveChangesAsync();
            await Task.WhenAll(fileCreationTasks);

            return videos;
        }

        public async Task<long> GetRemainingSpace()
        {
            long totalUsedSpace = await _videoManagerDbContext.Videos
                .AsNoTracking()
                .Where(x => x.CreatedByUserId == _userId && x.Status != VideoStatus.Deleted)
                .SumAsync(x => x.EncodedLength ?? x.OriginalLength);

            return totalAvailableSpaceInBytes - totalUsedSpace;
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
