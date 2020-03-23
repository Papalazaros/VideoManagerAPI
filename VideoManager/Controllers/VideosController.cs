using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        public VideosController(ILogger<VideosController> logger, IVideoService videoService)
        {
            _logger = logger;
            _videoService = videoService;
        }

        [HttpGet]
        [Route("random")]
        public async Task<List<Video>> GetRandomVideoId(int count)
        {
            return await _videoService.GetRandom(count);
        }

        [HttpGet]
        public async Task<List<Video>> GetAllVideos(VideoStatus? videoStatus)
        {
            return await _videoService.GetAll(videoStatus);
        }

        [HttpDelete]
        [Route("{videoId:Guid}")]
        public async Task<Video> Delete(Guid videoId)
        {
            return await _videoService.Delete(videoId);
        }

        [HttpDelete]
        [Route("deletefailed")]
        public async Task<IEnumerable<Video>> DeleteFailedFiles()
        {
            List<Video> failedVideos = await _videoService
                .GetAll(VideoStatus.Failed);

            return await _videoService.DeleteMany(failedVideos.Select(x => x.Id));
        }

        [HttpDelete]
        [Route("deleteduplicates")]
        public async Task<IEnumerable<Video>> DeleteDuplicateFiles()
        {
            List<Video> videos = await _videoService.GetAll();

            IEnumerable<Guid> duplicateVideoIds = videos
                .GroupBy(x => x.OriginalFileName)
                .Where(x => x.Count() > 1)
                .SelectMany(x => x.Skip(1))
                .Select(x => x.Id);

            return await _videoService.DeleteMany(duplicateVideoIds);
        }

        [HttpGet]
        [Route("{videoId:Guid}/stream")]
        public async Task<IActionResult> GetStream(Guid videoId)
        {
            Video video = await _videoService.Get(videoId);

            if (video == null || video.Status != VideoStatus.Ready)
            {
                return NoContent();
            }

            return PhysicalFile(Path.Join(Directory.GetCurrentDirectory(), video.GetEncodedFilePath()), "video/mp4", true);
        }

        [HttpGet]
        [Route("{videoId:Guid}")]
        public async Task<Video> GetVideoDetails(Guid videoId)
        {
            return await _videoService.Get(videoId);
        }

        [HttpPost]
        [Route("createmany")]
        public async Task<IActionResult> Post(IEnumerable<IFormFile> files)
        {
            Dictionary<string, IList<ValidationFailure>> failedFiles = new Dictionary<string, IList<ValidationFailure>>(9);
            VideoValidator videoValidator = new VideoValidator();

            foreach (IFormFile file in files)
            {
                ValidationResult validationResult = videoValidator.Validate(file);

                if (!validationResult.IsValid)
                {
                    failedFiles[file.FileName] = validationResult.Errors;
                }
            }

            IEnumerable<IFormFile> validVideos = files.Where(x => !failedFiles.ContainsKey(x.FileName));
            List<Video> createdVideos = await _videoService.CreateMany(validVideos);

            return Ok(new { failed = failedFiles, created = createdVideos });
        }

        [HttpPost]
        public async Task<IActionResult> Post(IFormFile file)
        {
            VideoValidator videoValidator = new VideoValidator();
            ValidationResult validationResult = videoValidator.Validate(file);

            if (!validationResult.IsValid)
            {
                return new BadRequestObjectResult(validationResult.Errors);
            }

            return Ok(await _videoService.Create(file));
        }
    }
}
