using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoManager.Models;
using VideoManager.Services;

namespace VideoManager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly ILogger<RoomController> _logger;
        private readonly IVideoService _videoService;
        private readonly IVideoSyncService _videoSyncService;

        public RoomController(ILogger<RoomController> logger,
            IVideoService videoService,
            IVideoSyncService videoSyncService)
        {
            _logger = logger;
            _videoService = videoService;
            _videoSyncService = videoSyncService;
        }

        [HttpPost]
        [Route("Sync")]
        public async Task<IActionResult> Post(VideoSyncMessage videoSyncMessage)
        {
            await _videoSyncService.SyncVideoAsync(videoSyncMessage);

            return Ok();
        }
    }
}
