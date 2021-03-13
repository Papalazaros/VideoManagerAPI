using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoManager.Models;
using VideoManager.Models.Database;

namespace VideoManager.Services
{
    public interface IRoomService
    {
        Task<Room?> Get(int roomId);
        Task<List<Room>> GetAll();
        Task<List<Room>> GetMemberships();
        Task<Room> Create(string name);
        Task<RoomVideo> AddVideo(Room room, int videoId);
        Task<RoomMember?> AddMember(Room room, string memberEmail);
        Task<bool> CanView(int roomId);
        Task<bool> CanEdit(int roomId);
        Task<Room> Delete(int roomId);
    }

    public class RoomService : IRoomService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly VideoManagerDbContext _videoManagerDbContext;

        private int? _userId => (int?)_httpContextAccessor.HttpContext?.Items["UserId"];

        public RoomService(VideoManagerDbContext videoManagerDbContext,
            IHttpContextAccessor httpContextAccessor)
        {
            _videoManagerDbContext = videoManagerDbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<Room?> Get(int roomId)
        {
            return _videoManagerDbContext.Rooms.FirstOrDefaultAsync(x => x.RoomId == roomId && x.CreatedByUserId == _userId)!;
        }

        public Task<List<Room>> GetAll()
        {
            return _videoManagerDbContext.Rooms.Where(x => x.CreatedByUserId == _userId).ToListAsync();
        }

        public Task<List<Room>> GetMemberships()
        {
            return _videoManagerDbContext.RoomMembers
                .Where(x => x.UserId == _userId)
                .Include(x => x.Room)
                .Where(x => x.Room!.RoomStatus == RoomStatus.Active)
                .Join(_videoManagerDbContext.Rooms, x => x.RoomId, x => x.RoomId, (_, room) => room)
                .ToListAsync();
        }

        public async Task<Room> Create(string name)
        {
            Room room = new(name);

            await _videoManagerDbContext.Rooms.AddAsync(room);
            await _videoManagerDbContext.SaveChangesAsync();
            return room;
        }

        public async Task<RoomMember?> AddMember(Room room, string memberEmail)
        {
            RoomMember? roomMember = null;
            User? user = await _videoManagerDbContext.Users.FirstOrDefaultAsync(x => x.Email == memberEmail.ToLower());

            if (user != null && user.UserId != _userId)
            {
                roomMember = await _videoManagerDbContext.RoomMembers.FirstOrDefaultAsync(x => x.UserId == user.UserId && x.RoomId == room.RoomId);

                if (roomMember == null)
                {
                    roomMember = new RoomMember(room.RoomId, user.UserId);

                    await _videoManagerDbContext.RoomMembers.AddAsync(roomMember);
                    await _videoManagerDbContext.SaveChangesAsync();
                }
            }

            return roomMember;
        }

        public async Task<bool> CanView(int roomId)
        {
            Room? room = await _videoManagerDbContext.Rooms.FirstOrDefaultAsync(x => x.RoomId == roomId && x.CreatedByUserId == _userId);
            RoomMember? roomMember = await _videoManagerDbContext.RoomMembers.FirstOrDefaultAsync(x => x.UserId == _userId && x.RoomId == roomId);

            return roomMember != null || room != null;
        }

        public async Task<bool> CanEdit(int roomId)
        {
            return await _videoManagerDbContext.Rooms.FirstOrDefaultAsync(x => x.RoomId == roomId && x.CreatedByUserId == _userId) != null;
        }

        public async Task<RoomVideo> AddVideo(Room room, int videoId)
        {
            RoomVideo roomVideo = new(room.RoomId, videoId);
            await _videoManagerDbContext.RoomVideos.AddAsync(roomVideo);
            await _videoManagerDbContext.SaveChangesAsync();

            return roomVideo;
        }

        public async Task<Room> Delete(int roomId)
        {
            Room? room = await _videoManagerDbContext.Rooms.FirstOrDefaultAsync(x => x.RoomId == roomId && x.CreatedByUserId == _userId);
            room.RoomStatus = RoomStatus.Inactive;
            await _videoManagerDbContext.SaveChangesAsync();
            return room;
        }
    }
}
