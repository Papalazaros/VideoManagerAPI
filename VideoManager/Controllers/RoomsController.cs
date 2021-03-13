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
        public async Task<List<Room>> Get()
        {
            return await _roomService.GetAll();
        }

        [HttpGet]
        [Route("Memberships")]
        public async Task<List<Room>> GetMemberships()
        {
            return await _roomService.GetMemberships();
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
        public async Task<RoomVideo> AddVideo(int roomId, int videoId)
        {
            return await _roomService.AddVideo(roomId, videoId);
        }

        [HttpGet]
        [Route("{roomId:int}/AddMember")]
        public async Task<RoomMember?> AddMember(int roomId, string memberEmail)
        {
            return await _roomService.AddMember(roomId, memberEmail);
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
        public async Task<Room> Delete(int roomId)
        {
            return await _roomService.Delete(roomId);
        }

        [HttpPost]
        public async Task<Room> Post(string name)
        {
            return await _roomService.Create(name);
        }
    }
}
