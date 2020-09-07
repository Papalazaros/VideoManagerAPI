using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoManager.Models;

namespace VideoManager.Services
{
    public interface IRoomService
    {
        Task<Room> Get(int roomId);
        Task<IEnumerable<Room>> GetAll();
    }

    public class RoomService : IRoomService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly VideoManagerDbContext _videoManagerDbContext;

        private int? UserId => (int?)_httpContextAccessor.HttpContext?.Items["UserId"];

        public RoomService(VideoManagerDbContext videoManagerDbContext,
            IHttpContextAccessor httpContextAccessor)
        {
            _videoManagerDbContext = videoManagerDbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Room> Get(int roomId)
        {
            return await _videoManagerDbContext.Rooms.FirstOrDefaultAsync(x => x.RoomId == roomId);
        }

        public async Task<IEnumerable<Room>> GetAll()
        {
            return await _videoManagerDbContext.Rooms.Where(x => x.CreatedByUserId == UserId).ToListAsync();
        }
    }
}
