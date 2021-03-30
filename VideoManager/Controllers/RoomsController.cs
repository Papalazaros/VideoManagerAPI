using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VideoManager.Models;
using VideoManager.Services;

namespace VideoManager.Controllers
{
    [Produces("application/json")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
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
        public Task<IEnumerable<Room>> Get()
        {
            return _roomService.GetAll();
        }

        [HttpGet]
        [Route("Memberships")]
        public Task<IEnumerable<Room>> GetMemberships()
        {
            return _roomService.GetMemberships();
        }

        [HttpGet]
        [Route("{roomId:int}")]
        public async Task<IActionResult> Get(int roomId)
        {
            (bool canView, Room room) = await _roomService.CanView(roomId);
            if (room == null) return NotFound();
            if (!canView) return Unauthorized();
            return Ok(room);
        }

        [HttpGet]
        [Route("{roomId:int}/AddVideo")]
        public Task<RoomVideo> AddVideo(int roomId, int videoId)
        {
            return _roomService.AddVideo(roomId, videoId);
        }

        [HttpGet]
        [Route("{roomId:int}/AddMember")]
        public Task<RoomMember?> AddMember(int roomId, string memberEmail)
        {
            return _roomService.AddMember(roomId, memberEmail);
        }

        [HttpGet]
        [Route("{roomId:int}/CanView")]
        public async Task<bool> CanView(int roomId)
        {
            return (await _roomService.CanView(roomId)).canView;
        }

        [HttpGet]
        [Route("{roomId:int}/CanEdit")]
        public async Task<bool> CanEdit(int roomId)
        {
            return (await _roomService.CanEdit(roomId)).canEdit;
        }

        [HttpDelete]
        [Route("{roomId:int}")]
        public Task<Room> Delete(int roomId)
        {
            return _roomService.Delete(roomId);
        }

        [HttpPost]
        public Task<Room> Post(string name)
        {
            return _roomService.Create(name);
        }
    }
}
