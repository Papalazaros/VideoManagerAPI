using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VideoManager.Models;

namespace VideoManager.Services
{
    public class VideoHub : Hub
    {
        private readonly IRoomService _roomService;
        private readonly IAuth0Service _auth0Service;
        private readonly IUserService _userService;
        private readonly VideoManagerDbContext _videoManagerDbContext;
        //private readonly Dictionary<string, Dictionary<string, string>> roomMembers = new Dictionary<string, Dictionary<string, string>>();

        public VideoHub(IRoomService roomService,
            IAuth0Service auth0Service,
            IUserService userService,
            VideoManagerDbContext videoManagerDbContext)
        {
            _roomService = roomService;
            _auth0Service = auth0Service;
            _userService = userService;
            _videoManagerDbContext = videoManagerDbContext;
        }

        public async Task ReceiveSyncMessage(Guid roomId, string auth0Token, VideoSyncMessage videoSyncMessage)
        {
            Room room = await _roomService.Get(roomId);
            string auth0UserId = await _auth0Service.GetAuth0UserId(auth0Token);
            Guid? userId = await _userService.GetUserIdByAuthId(auth0UserId);

            if ((!room.OwnerId.HasValue || room.OwnerId == userId) && userId.HasValue)
            {
                if (!room.OwnerId.HasValue)
                {
                    room.OwnerId = userId;
                }

                await Clients.OthersInGroup(roomId.ToString()).SendAsync("VideoSyncMessage", videoSyncMessage);
            }
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {

            return base.OnDisconnectedAsync(exception);
        }

        public async Task<string> JoinRoom(string roomName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

            return Context.ConnectionId;
        }
    }
}
