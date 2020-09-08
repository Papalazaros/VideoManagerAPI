﻿using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
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
            IAuthService auth0Service,
            IUserService userService)
        {
            _roomService = roomService;
            _authService = auth0Service;
            _userService = userService;
        }

        public async Task ReceiveSyncMessage(int roomId, string auth0Token, VideoSyncMessage videoSyncMessage)
        {
            Room room = await _roomService.Get(roomId);
            string auth0UserId = await _authService.GetUserId(auth0Token);
            User user = await _userService.CreateOrGetByAuthId(auth0UserId);

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
