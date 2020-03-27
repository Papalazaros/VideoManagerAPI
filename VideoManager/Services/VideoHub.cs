using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using VideoManager.Models;

namespace VideoManager.Services
{
    public class VideoHub : Hub
    {
        //public async Task ReceiveSyncMessage(VideoSyncMessage videoSyncRequest)
        //{
        //    await Clients.All.SendAsync("VideoSyncMessage", videoSyncRequest);
        //}
    }
}
