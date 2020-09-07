using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using VideoManager.Models;

namespace VideoManager.Services
{
    public class VideoHub : Hub
    {
        private readonly IRoomService _roomService;
        private readonly IAuth0Service _auth0Service;
        private readonly IUserService _userService;

        public VideoHub(IRoomService roomService,
            IAuth0Service auth0Service,
            IUserService userService)
        {
            _roomService = roomService;
            _auth0Service = auth0Service;
            _userService = userService;
        }

        public async Task ReceiveSyncMessage(int syncId, string auth0Token, VideoSyncMessage videoSyncMessage)
        {
            Room room = await _roomService.Get(syncId);
            string auth0UserId = await _auth0Service.GetAuth0UserId(auth0Token);
            int? userId = await _userService.GetUserIdByAuthId(auth0UserId);

            if (room != null)
            {
                //if ((!room.OwnerId.HasValue || room.OwnerId == userId) && userId.HasValue)
                //{
                //    if (!room.OwnerId.HasValue)
                //    {
                //        room.OwnerId = userId;
                //    }

                //    await Clients.OthersInGroup(syncId.ToString()).SendAsync("VideoSyncMessage", videoSyncMessage);
                //}
                await Clients.OthersInGroup(syncId.ToString()).SendAsync("VideoSyncMessage", videoSyncMessage);
            }
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return base.OnDisconnectedAsync(exception);
        }

        public async Task<string> JoinRoom(int syncId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, syncId.ToString());

            return Context.ConnectionId;
        }
    }
}
