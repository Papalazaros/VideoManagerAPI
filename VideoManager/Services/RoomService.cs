using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoManager.Exceptions;
using VideoManager.Models;
using VideoManager.Models.Database;

namespace VideoManager.Services
{
    public interface IRoomService
    {
        Task<Room> Get(int roomId);
        Task<IEnumerable<Room>> GetAll();
        Task<IEnumerable<Room>> GetMemberships();
        Task<Room> Create(string name);
        Task<RoomVideo> AddVideo(int roomId, int videoId);
        Task<RoomMember?> AddMember(int roomId, string memberEmail);
        Task<(bool canView, Room room)> CanView(int roomId);
        Task<(bool canEdit, Room room)> CanEdit(int roomId);
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

        public async Task<Room> Get(int roomId)
        {
            Room room = await _videoManagerDbContext.Rooms.FindAsync(roomId);
            if (room == null || room.RoomStatus == RoomStatus.Inactive) throw new NotFoundException();
            return room;
        }

        public async Task<IEnumerable<Room>> GetAll()
        {
            return await _videoManagerDbContext.Rooms
                .AsNoTracking()
                .Where(x => x.CreatedByUserId == _userId && x.RoomStatus == RoomStatus.Active)
                .ToListAsync();
        }

        public async Task<IEnumerable<Room>> GetMemberships()
        {
            return await _videoManagerDbContext.RoomMembers
                .AsNoTracking()
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

        public async Task<RoomMember?> AddMember(int roomId, string memberEmail)
        {
            (bool canEdit, Room room) = await CanEdit(roomId);
            if (!canEdit) throw new UnauthorizedAccessException();
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

        public async Task<(bool canView, Room room)> CanView(int roomId)
        {
            Room? room = await _videoManagerDbContext.Rooms.FindAsync(roomId);
            if (room == null || room.RoomStatus == RoomStatus.Inactive) throw new NotFoundException();
            RoomMember? roomMember = await _videoManagerDbContext.RoomMembers.FirstOrDefaultAsync(x => x.UserId == _userId && x.RoomId == roomId);

            return (roomMember != null || room.CreatedByUserId == _userId || !room.IsPrivate, room);
        }

        public async Task<(bool canEdit, Room room)> CanEdit(int roomId)
        {
            Room? room = await _videoManagerDbContext.Rooms.FindAsync(roomId);
            if (room == null || room.RoomStatus == RoomStatus.Inactive) throw new NotFoundException();
            return (room.CreatedByUserId == _userId, room);
        }

        public async Task<RoomVideo> AddVideo(int roomId, int videoId)
        {
            (bool canEdit, Room room) = await CanEdit(roomId);
            if (!canEdit) throw new UnauthorizedAccessException();
            RoomVideo roomVideo = new(room.RoomId, videoId);
            await _videoManagerDbContext.RoomVideos.AddAsync(roomVideo);
            await _videoManagerDbContext.SaveChangesAsync();

            return roomVideo;
        }

        public async Task<Room> Delete(int roomId)
        {
            (bool canEdit, Room room) = await CanEdit(roomId);
            if (!canEdit) throw new UnauthorizedAccessException();
            room.RoomStatus = RoomStatus.Inactive;
            await _videoManagerDbContext.SaveChangesAsync();
            return room;
        }
    }
}
