using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public class RoomsController : ControllerBase
    {
        private readonly ILogger<RoomsController> _logger;
        private readonly IVideoService _videoService;
        private readonly IVideoSyncService _videoSyncService;
        private readonly VideoManagerDbContext _videoManagerDbContext;

        public RoomsController(ILogger<RoomsController> logger,
            IVideoService videoService,
            IVideoSyncService videoSyncService,
            VideoManagerDbContext videoManagerDbContext)
        {
            _logger = logger;
            _videoService = videoService;
            _videoSyncService = videoSyncService;
            _videoManagerDbContext = videoManagerDbContext;
        }

        [HttpGet]
        [Route("{roomId:Guid}/Videos")]
        public async Task<IActionResult> GetRoomVideos(Guid roomId)
        {
            List<Video> allVideos = await _videoService.GetAll();

            return Ok(await _videoManagerDbContext.Rooms.Include(x => x.Videos).FirstOrDefaultAsync(x => x.Id == roomId));
        }

        [HttpGet]
        public async Task<IActionResult> Get(Guid userId)
        {
            return Ok(await _videoManagerDbContext.Rooms.Where(x => x.CreatedById == userId).ToListAsync());
        }

        [HttpPost]
        [Route("{roomId:Guid}/Sync")]
        public async Task<IActionResult> Post(Guid roomId, [FromQuery]Guid userId, [FromBody]VideoSyncMessage videoSyncMessage)
        {
            Room room = await _videoManagerDbContext.Rooms.FirstOrDefaultAsync(x => (!x.OwnerId.HasValue || x.OwnerId == userId) && x.Id == roomId);

            if (room != null)
            {
                await _videoSyncService.SyncVideoAsync(roomId, videoSyncMessage);
                return Ok();
            }

            return BadRequest();
        }
    }
}
