using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly IRoomService _roomService;

        public RoomsController(ILogger<RoomsController> logger,
            IRoomService roomService)
        {
            _logger = logger;
            _roomService = roomService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _roomService.GetAll());
        }

        [HttpGet]
        [Route("{roomId:int}")]
        public async Task<IActionResult> Get(int roomId)
        {
            return Ok(await _roomService.Get(roomId));
        }

        [HttpGet]
        [Route("{roomId:int}/Videos")]
        public async Task<IActionResult> GetVideos(int roomId)
        {
            return Ok(await _roomService.Get(roomId));
        }

        [HttpPost]
        public async Task<IActionResult> Post(string name)
        {
            Room room = await _roomService.Create(name);
            return Ok(room);
        }
    }
}
