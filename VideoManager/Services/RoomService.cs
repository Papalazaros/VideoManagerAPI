using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoManager.Models;

namespace VideoManager.Services
{
    public interface IRoomService
    {
        Task<Room> Get(Guid id);
    }

    public class RoomService : IRoomService
    {
        private readonly VideoManagerDbContext _videoManagerDbContext;

        public RoomService(VideoManagerDbContext videoManagerDbContext)
        {
            _videoManagerDbContext = videoManagerDbContext;
        }

        public async Task<Room> Get(Guid roomId)
        {
            return await _videoManagerDbContext.Rooms.FindAsync(roomId);
        }
    }
}
