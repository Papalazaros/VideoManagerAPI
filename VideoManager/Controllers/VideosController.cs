using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VideoManager.Models;
using VideoManager.Services;
using VideoManager.Validators;

namespace VideoManager.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [Route("[controller]")]
    public class VideosController : ControllerBase
    {
        private readonly IUserVideoService _videoService;

        public VideosController(IUserVideoService videoService)
        {
            _videoService = videoService;
        }

        [HttpGet]
        public Task<List<Video>> GetAll(int? roomId, VideoStatus? videoStatus)
        {
            return _videoService.GetAll(roomId, videoStatus);
        }

        [HttpDelete]
        [Route("{videoId:int}")]
        public async Task<Video?> Delete(int videoId)
        {
            return await _videoService.Delete(videoId);
        }

        [HttpGet]
        [Route("{videoId:int}/Stream")]
        public async Task<IActionResult> GetStream(int videoId)
        {
            Video video = await _videoService.Get(videoId);
            string videoPath = Path.GetFullPath(video.GetEncodedFilePath());

            if (HttpContext.Request.GetTypedHeaders().Range == null) return BadRequest();

            return PhysicalFile(videoPath, "video/mp4", true);
        }

        [HttpGet]
        [Route("{videoId:int}/Thumbnail")]
        public async Task<IActionResult> GetThumbnail(int videoId)
        {
            Video video = await _videoService.Get(videoId);

            if (string.IsNullOrEmpty(video?.ThumbnailFilePath)) return NotFound();

            string thumbnailPath = Path.GetFullPath(video.ThumbnailFilePath);

            return PhysicalFile(thumbnailPath, "image/webp");
        }

        [HttpGet]
        [Route("{videoId:int}/Preview")]
        public async Task<IActionResult> GetPreview(int videoId)
        {
            Video video = await _videoService.Get(videoId);

            if (string.IsNullOrEmpty(video?.PreviewFilePath)) return NotFound();

            string previewPath = Path.GetFullPath(video.PreviewFilePath);

            return PhysicalFile(previewPath, "image/webp");
        }

        [HttpGet]
        [Route("{videoId:int}")]
        public async Task<Video> Get(int videoId)
        {
            return await _videoService.Get(videoId);
        }

        [HttpPost]
        public async Task<IActionResult> Post(IEnumerable<IFormFile> files)
        {
            if (files == null) return BadRequest();
            long remainingSpace = await _videoService.GetRemainingSpace();

            Dictionary<string, IEnumerable<string>> failedFiles = new();
            VideoValidator videoValidator = new();

            foreach (IFormFile file in files)
            {
                ValidationResult validationResult = videoValidator.Validate(file);

                if (!validationResult.IsValid) failedFiles[file.FileName] = validationResult.Errors.Select(x => x.ErrorMessage);
                else if (remainingSpace - file.Length <= 0) failedFiles[file.FileName] = new List<string> { "No space remaining." };
                else if (await _videoService.FindByOriginalVideoName(file.FileName) != null) failedFiles[file.FileName] = new List<string> { "Duplicate file." };
                else remainingSpace -= file.Length;
            }

            IEnumerable<IFormFile> validVideos = files.Where(x => !failedFiles.ContainsKey(x.FileName));
            IEnumerable<Video> createdVideos = await _videoService.CreateMany(validVideos);

            return Ok(new { failed = failedFiles, created = createdVideos });
        }
    }
}
