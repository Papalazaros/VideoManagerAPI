using AutoMapper;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VideoManager.Models;
using VideoManager.Models.Dto;
using VideoManager.Services;
using VideoManager.Validators;

namespace VideoManager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VideosController : ControllerBase
    {
        private readonly IUserVideoService _videoService;
        private readonly IMapper _mapper;
        private int? _userId => (int?)HttpContext?.Items["UserId"];

        public VideosController(IUserVideoService videoService,
            IMapper mapper)
        {
            _videoService = videoService;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<IEnumerable<Video>> GetAll(int? roomId, VideoStatus? videoStatus)
        {
            return await _videoService.GetAll(_userId, roomId, videoStatus);
        }

        [HttpDelete]
        [Route("{videoId:int}")]
        public async Task<IActionResult> Delete(int videoId)
        {
            Video? video = await _videoService.Get(videoId);

            if (video == null) return NotFound();

            return Ok(await _videoService.Delete(_userId, videoId));
        }

        [HttpGet]
        [Route("{videoId:int}/Stream")]
        public async Task<IActionResult> GetStream(int videoId)
        {
            Video? video = await _videoService.Get(videoId);

            if (video == null) return NotFound();

            string videoPath = Path.Join(Directory.GetCurrentDirectory(), video?.GetEncodedFilePath());

            return PhysicalFile(videoPath, "video/mp4", true);
        }

        [HttpGet]
        [Route("{videoId:int}/Thumbnail")]
        public async Task<IActionResult> GetThumbnail(int videoId)
        {
            Video? video = await _videoService.Get(videoId);

            if (video == null) return NotFound();

            string thumbnailPath = Path.Join(Directory.GetCurrentDirectory(), video?.ThumbnailFilePath);

            return PhysicalFile(thumbnailPath, "image/jpeg");
        }

        [HttpGet]
        [Route("{videoId:int}")]
        public async Task<IActionResult> Get(int videoId)
        {
            Video? video = await _videoService.Get(videoId);

            if (video == null) return NotFound();

            return Ok(video);
        }

        [HttpPost]
        [Route("CreateMany")]
        public async Task<IActionResult> Post(IEnumerable<IFormFile> files)
        {
            if (files == null) return BadRequest();

            Dictionary<string, IEnumerable<string>> failedFiles = new();
            VideoValidator videoValidator = new();

            foreach (IFormFile file in files)
            {
                ValidationResult validationResult = videoValidator.Validate(file);

                if (!validationResult.IsValid) failedFiles[file.FileName] = validationResult.Errors.Select(x => x.ErrorMessage);
                else if (await _videoService.FindByOriginalVideoName(_userId, file.FileName) != null) failedFiles[file.FileName] = new List<string> { "Duplicate file." };
            }

            IEnumerable<IFormFile> validVideos = files.Where(x => !failedFiles.ContainsKey(x.FileName));
            List<Video> createdVideos = await _videoService.CreateMany(validVideos);

            return Ok(new { failed = failedFiles, created = createdVideos });
        }

        [HttpPost]
        public async Task<IActionResult> Post(IFormFile file)
        {
            if (file == null) return BadRequest();

            VideoValidator videoValidator = new();
            ValidationResult validationResult = videoValidator.Validate(file);

            if (!validationResult.IsValid) return new BadRequestObjectResult(validationResult.Errors.Select(x => x.ErrorMessage));
            else if (await _videoService.FindByOriginalVideoName(_userId, file.FileName) != null) return BadRequest(new List<string> { "Duplicate file." });

            return Ok(_mapper.Map<Video, PostVideoDto>(await _videoService.Create(file)));
        }
    }
}
