using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
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
    [Route("[controller]")]
    public class VideosController : ControllerBase
    {
        private readonly ILogger<VideosController> _logger;
        private readonly IVideoService _videoService;
        private readonly IMemoryCache _memoryCache;

        public VideosController(ILogger<VideosController> logger,
            IVideoService videoService,
            IMemoryCache memoryCache)
        {
            _logger = logger;
            _videoService = videoService;
            _memoryCache = memoryCache;
        }

        [HttpGet]
        [Route("Random")]
        public async Task<List<Video>> GetRandomVideoId(int count)
        {
            return await _videoService.GetRandom(count);
        }

        [HttpGet]
        public async Task<IEnumerable<Video>> GetAllVideos(VideoStatus? videoStatus)
        {
            return await _videoService.GetAll(videoStatus);
        }

        [HttpDelete]
        [Route("{videoId:Guid}")]
        public async Task<Video> Delete(Guid videoId)
        {
            return await _videoService.Delete(videoId);
        }

        [HttpGet]
        [Route("{videoId:Guid}/Stream")]
        public async Task<IActionResult> GetStream(Guid videoId)
        {
            var requestedRange = HttpContext.Request.GetTypedHeaders().Range?.Ranges;

            if (requestedRange == null) return BadRequest();

            Video video = await _videoService.Get(videoId);
            if (video == null) return NoContent();
            string videoPath = Path.Join(Directory.GetCurrentDirectory(), video?.GetEncodedFilePath());

            return PhysicalFile(videoPath, "video/mp4", true);
        }

        [HttpGet]
        [Route("{videoId:Guid}/Thumbnail")]
        public async Task<IActionResult> GetThumbnail(Guid videoId)
        {
            Video video = await _videoService.Get(videoId);

            if (video == null) return NoContent();
            string thumbnailPath = Path.Join(Directory.GetCurrentDirectory(), video?.ThumbnailFilePath);
            return PhysicalFile(thumbnailPath, "image/jpeg");
        }

        [HttpGet]
        [Route("{videoId:Guid}")]
        public async Task<Video> GetVideoDetails(Guid videoId)
        {
            return await _videoService.Get(videoId);
        }

        [HttpPost]
        [Route("CreateMany")]
        public async Task<IActionResult> Post(IEnumerable<IFormFile> files)
        {
            if (files == null) return BadRequest();

            Dictionary<string, IList<ValidationFailure>> failedFiles = new Dictionary<string, IList<ValidationFailure>>(9);
            VideoValidator videoValidator = new VideoValidator();

            foreach (IFormFile file in files)
            {
                ValidationResult validationResult = videoValidator.Validate(file);

                if (!validationResult.IsValid) failedFiles[file.FileName] = validationResult.Errors;
            }

            IEnumerable<IFormFile> validVideos = files.Where(x => !failedFiles.ContainsKey(x.FileName));
            List<Video> createdVideos = await _videoService.CreateMany(validVideos);

            return Ok(new { failed = failedFiles, created = createdVideos });
        }

        [HttpPost]
        public async Task<IActionResult> Post(IFormFile file)
        {
            if (file == null) return BadRequest();

            VideoValidator videoValidator = new VideoValidator();
            ValidationResult validationResult = videoValidator.Validate(file);

            if (!validationResult.IsValid) return new BadRequestObjectResult(validationResult.Errors);

            return Ok(await _videoService.Create(file));
        }
    }
}
