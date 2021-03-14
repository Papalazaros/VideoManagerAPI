﻿using AutoMapper;
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
        public async Task<Video?> Delete(int videoId)
        {
            return await _videoService.Delete(_userId, videoId);
        }

        [HttpGet]
        [Route("{videoId:int}/Stream")]
        public async Task<IActionResult> GetStream(int videoId)
        {
            Video video = await _videoService.Get(videoId);
            string videoPath = Path.Join(Directory.GetCurrentDirectory(), video?.GetEncodedFilePath());

            if (!System.IO.File.Exists(videoPath)) return NotFound();
            if (HttpContext.Request.GetTypedHeaders().Range == null) return BadRequest();

            return PhysicalFile(videoPath, "video/mp4", true);
        }

        [HttpGet]
        [Route("{videoId:int}/Thumbnail")]
        public async Task<IActionResult> GetThumbnail(int videoId)
        {
            Video video = await _videoService.Get(videoId);

            if (string.IsNullOrEmpty(video?.ThumbnailFilePath)) return NotFound();

            string thumbnailPath = Path.Join(Directory.GetCurrentDirectory(), video.ThumbnailFilePath);

            if (!System.IO.File.Exists(thumbnailPath)) return NotFound();

            return PhysicalFile(thumbnailPath, "image/jpeg");
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
            long remainingSpace = await _videoService.GetRemainingSpace(_userId);

            Dictionary<string, IEnumerable<string>> failedFiles = new();
            VideoValidator videoValidator = new();

            foreach (IFormFile file in files)
            {
                ValidationResult validationResult = videoValidator.Validate(file);

                if (!validationResult.IsValid) failedFiles[file.FileName] = validationResult.Errors.Select(x => x.ErrorMessage);
                else if (remainingSpace - file.Length <= 0) failedFiles[file.FileName] = new List<string> { "No space remaining." };
                else if (await _videoService.FindByOriginalVideoName(_userId, file.FileName) != null) failedFiles[file.FileName] = new List<string> { "Duplicate file." };
                else remainingSpace -= file.Length;
            }

            IEnumerable<IFormFile> validVideos = files.Where(x => !failedFiles.ContainsKey(x.FileName));
            List<Video> createdVideos = await _videoService.CreateMany(validVideos);

            return Ok(new { failed = failedFiles, created = createdVideos });
        }
    }
}
