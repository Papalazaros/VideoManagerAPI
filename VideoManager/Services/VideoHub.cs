using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using VideoManager.Models;

namespace VideoManager.Services
{
    public class VideoHub : Hub
    {
        private readonly RoomService _roomService;
        private readonly IAuth0Service _auth0Service;
        private readonly IUserService _userService;

        public VideoHub(RoomService roomService,
            IAuth0Service auth0Service,
            IUserService userService)
        {
            _roomService = roomService;
            _auth0Service = auth0Service;
            _userService = userService;
        }

        public async Task ReceiveSyncMessage(string auth0Token, VideoSyncMessage videoSyncMessage)
        {
            Room room = await _roomService.Get(videoSyncMessage.RoomId);
            string auth0UserId = await _auth0Service.GetAuth0UserId(auth0Token);
            Guid? userId = await _userService.GetUserId(auth0UserId);

            if (!room.OwnerId.HasValue || room.OwnerId == userId)
            {
                await Clients.GroupExcept(videoSyncMessage.RoomId.ToString(), Context.ConnectionId).SendAsync("VideoSyncMessage", videoSyncMessage);
            }
        }

        public async Task<string> JoinRoom(string roomName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

            return Context.ConnectionId;
        }
    }
}
