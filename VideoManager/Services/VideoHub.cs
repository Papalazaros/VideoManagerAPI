using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using VideoManager.Models;

namespace VideoManager.Services
{
    public class VideoHub : Hub
    {
        private readonly RoomService _roomService;

        public VideoHub(RoomService roomService)
        {
            _roomService = roomService;
        }

        public async Task ReceiveSyncMessage(Guid userId, VideoSyncMessage videoSyncMessage)
        {
            Room room = await _roomService.Get(videoSyncMessage.RoomId);

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
