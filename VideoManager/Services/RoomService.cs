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
        Task<List<Room>> GetAll();
        Task<List<Room>> GetMemberships();
        Task<Room> Create(string name);
        Task<RoomVideo> AddVideo(int roomId, int videoId);
        Task<RoomMember> AddMember(int roomId, string memberEmail);
        Task<bool> CanViewRoom(int roomId);
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

        public async Task<Room> Get(int roomId)
        {
            return await _videoManagerDbContext.Rooms.FirstOrDefaultAsync(x => x.RoomId == roomId && x.CreatedByUserId == _userId);
        }

        public async Task<List<Room>> GetAll()
        {
            return await _videoManagerDbContext.Rooms.Where(x => x.CreatedByUserId == _userId).ToListAsync();
        }

        public async Task<List<Room>> GetMemberships()
        {
            return await _videoManagerDbContext.RoomMembers.Where(x => x.UserId == _userId).Join(_videoManagerDbContext.Rooms, x => x.RoomId, x => x.RoomId, (_, room) => room).ToListAsync();
        }

        public async Task<Room> Create(string name)
        {
            Room room = new Room
            {
                Name = name
            };

            await _videoManagerDbContext.Rooms.AddAsync(room);
            await _videoManagerDbContext.SaveChangesAsync();
            return room;
        }

        public async Task<RoomMember> AddMember(int roomId, string memberEmail)
        {
            RoomMember roomMember = null;
            Room room = await _videoManagerDbContext.Rooms.FirstOrDefaultAsync(x => x.RoomId == roomId && x.CreatedByUserId == _userId);
            User user = await _videoManagerDbContext.Users.FirstOrDefaultAsync(x => x.Email == memberEmail.ToLower());

            if (room != null && user != null && user.UserId != _userId)
            {
                roomMember = await _videoManagerDbContext.RoomMembers.FirstOrDefaultAsync(x => x.UserId == user.UserId && x.RoomId == room.RoomId);

                if (roomMember == null)
                {
                    roomMember = new RoomMember
                    {
                        RoomId = roomId,
                        UserId = user.UserId
                    };

                    await _videoManagerDbContext.RoomMembers.AddAsync(roomMember);
                    await _videoManagerDbContext.SaveChangesAsync();
                }
            }

            return roomMember;
        }

        public async Task<bool> CanViewRoom(int roomId)
        {
            Room room = await _videoManagerDbContext.Rooms.FirstOrDefaultAsync(x => x.RoomId == roomId && x.CreatedByUserId == _userId);
            RoomMember roomMember = await _videoManagerDbContext.RoomMembers.FirstOrDefaultAsync(x => x.UserId == _userId && x.RoomId == roomId);
            return roomMember != null || room != null;
        }

        public async Task<RoomVideo> AddVideo(int roomId, int videoId)
        {
            RoomVideo roomVideo = new RoomVideo { RoomId = roomId, VideoId = videoId };
            await _videoManagerDbContext.RoomVideos.AddAsync(roomVideo);
            await _videoManagerDbContext.SaveChangesAsync();

            return roomVideo;
        }
    }
}
