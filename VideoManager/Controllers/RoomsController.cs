using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VideoManager.Models;
using VideoManager.Services;

namespace VideoManager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomsController(IRoomService roomService)
        {
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
        public async Task<IActionResult> CanView(int roomId)
        {
            return Ok((await _roomService.CanView(roomId)).canView);
        }

        [HttpGet]
        [Route("{roomId:int}/CanEdit")]
        public async Task<IActionResult> CanEdit(int roomId)
        {
            return Ok((await _roomService.CanEdit(roomId)).canEdit);
        }

        [HttpDelete]
        [Route("{roomId:int}")]
        public async Task<IActionResult> Delete(int roomId)
        {
            return Ok(await _roomService.Delete(roomId));
        }

        [HttpPost]
        public async Task<IActionResult> Post(string name)
        {
            return Ok(await _roomService.Create(name));
        }
    }
}
