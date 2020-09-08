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
        [Route("Memberships")]
        public async Task<IActionResult> GetMemberships()
        {
            return Ok(await _roomService.GetMemberships());
        }

        [HttpGet]
        [Route("{roomId:int}")]
        public async Task<IActionResult> Get(int roomId)
        {
            return Ok(await _roomService.Get(roomId));
        }

        [HttpGet]
        [Route("{roomId:int}/AddVideo")]
        public async Task<IActionResult> AddVideo(int roomId, int videoId)
        {
            return Ok(await _roomService.AddVideo(roomId, videoId));
        }

        [HttpGet]
        [Route("{roomId:int}/AddMember")]
        public async Task<IActionResult> AddMember(int roomId, string memberEmail)
        {
            return Ok(await _roomService.AddMember(roomId, memberEmail));
        }

        [HttpGet]
        [Route("{roomId:int}/CanView")]
        public async Task<IActionResult> CanViewRoom(int roomId)
        {
            return Ok(await _roomService.CanViewRoom(roomId));
        }

        [HttpPost]
        public async Task<IActionResult> Post(string name)
        {
            Room room = await _roomService.Create(name);
            return Ok(room);
        }
    }
}
