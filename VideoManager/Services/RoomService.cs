using Microsoft.EntityFrameworkCore;
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
        Task<IEnumerable<Room>> GetAll(Guid userId);
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

        public async Task<IEnumerable<Room>> GetAll(Guid userId)
        {
            return await _videoManagerDbContext.Rooms.Where(x => x.CreatedById == userId).ToListAsync();
        }
    }
}
