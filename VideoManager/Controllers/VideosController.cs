using AutoMapper;
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
using VideoManager.Models.Dto;
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
        private readonly IAuth0Service _auth0Service;
        private readonly IUserService _userService;
        private readonly IMemoryCache _memoryCache;
        private readonly IMapper _mapper;

        public VideosController(ILogger<VideosController> logger,
            IVideoService videoService,
            IAuth0Service auth0Service,
            IUserService userService,
            IMemoryCache memoryCache,
            IMapper mapper)
        {
            _logger = logger;
            _videoService = videoService;
            _auth0Service = auth0Service;
            _userService = userService;
            _memoryCache = memoryCache;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("Random")]
        public async Task<IEnumerable<GetVideoDto>> GetRandomVideoId(int count)
        {
            return _mapper.Map<List<Video>, List<GetVideoDto>>(await _videoService.GetRandom(count));
        }

        [HttpGet]
        public async Task<IEnumerable<GetVideoDto>> GetAllVideos(VideoStatus? videoStatus)
        {
            return _mapper.Map<List<Video>, List<GetVideoDto>>(await _videoService.GetAll(videoStatus));
        }

        [HttpDelete]
        [Route("{videoId:Guid}")]
        public async Task<Video> Delete(Guid videoId)
        {
            return await _videoService.Delete(videoId);
        }

        [HttpGet]
        [Route("{videoId:Guid}/Stream")]
        public async Task<IActionResult> GetStream(string accessToken, Guid videoId)
        {
            Video video = await _videoService.Get(videoId);

            if (video == null) return NoContent();

            string videoPath = Path.Join(Directory.GetCurrentDirectory(), video?.GetEncodedFilePath());

            return PhysicalFile(videoPath, "video/mp4", true);
        }

        [HttpGet]
        [Route("{videoId:Guid}/Thumbnail")]
        public async Task<IActionResult> GetThumbnail(string accessToken, Guid videoId)
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

            Dictionary<string, IEnumerable<string>> failedFiles = new Dictionary<string, IEnumerable<string>>();
            VideoValidator videoValidator = new VideoValidator();

            foreach (IFormFile file in files)
            {
                ValidationResult validationResult = videoValidator.Validate(file);

                if (!validationResult.IsValid) failedFiles[file.FileName] = validationResult.Errors.Select(x => x.ErrorMessage);
                else if (await _videoService.FindByOriginalVideoName(file.FileName) != null) failedFiles[file.FileName] = new List<string> { "Duplicate file." };
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

            if (!validationResult.IsValid) return new BadRequestObjectResult(validationResult.Errors.Select(x => x.ErrorMessage));
            else if (await _videoService.FindByOriginalVideoName(file.FileName) != null) return BadRequest(new List<string> { "Duplicate file." });

            return Ok(_mapper.Map<Video, PostVideoDto>(await _videoService.Create(file)));
        }
    }
}
