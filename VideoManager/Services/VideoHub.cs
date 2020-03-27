using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace VideoManager.Services
{
    public class VideoHub : Hub
    {
        public async Task JoinRoom(string roomName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        }
    }
}
