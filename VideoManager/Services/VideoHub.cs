﻿using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using VideoManager.Models;

namespace VideoManager.Services
{
    public class VideoHub : Hub
    {
        private readonly IRoomService _roomService;
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public VideoHub(IRoomService roomService,
            IAuthService authService,
            IUserService userService)
        {
            _roomService = roomService;
            _authService = authService;
            _userService = userService;
        }

        public async Task ReceiveSyncMessage(int roomId, string authToken, VideoSyncMessage videoSyncMessage)
        {
            Room room = await _roomService.Get(roomId);
            AuthUser authUser = await _authService.GetUser(authToken);
            User user = await _userService.CreateOrGetByAuthUser(authUser);

            if (room != null && user != null && room.CreatedByUserId == user.UserId)
            {
                await Clients.OthersInGroup(roomId.ToString()).SendAsync("VideoSyncMessage", videoSyncMessage);
            }
        }

        public async Task<string> JoinRoom(int roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
            return Context.ConnectionId;
        }
    }
}
