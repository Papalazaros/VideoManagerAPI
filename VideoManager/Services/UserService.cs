using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoManager.Models;

namespace VideoManager.Services
{
    public interface IUserService
    {
        Task<User> CreateOrGetByAuthId(string auth0Id);
        Task<Guid?> GetUserIdByAuthId(string auth0Id);
    }

    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly VideoManagerDbContext _videoManagerDbContext;
        private readonly IMemoryCache _memoryCache;

        public UserService(ILogger<UserService> logger,
            VideoManagerDbContext videoManagerDbContext,
            IMemoryCache memoryCache)
        {
            _logger = logger;
            _videoManagerDbContext = videoManagerDbContext;
            _memoryCache = memoryCache;
        }

        public async Task<User> CreateOrGetByAuthId(string auth0Id)
        {
            if (string.IsNullOrEmpty(auth0Id)) return null;
            if (_memoryCache.TryGetValue(auth0Id, out User user)) return user;

            user = await _videoManagerDbContext.Users.FirstOrDefaultAsync(x => x.Auth0Id == auth0Id);

            if (user == null)
            {
                Guid userId = Guid.NewGuid();
                Guid roomId = Guid.NewGuid();
                Guid playlistId = Guid.NewGuid();

                user = new User
                {
                    UserId = userId,
                    Auth0Id = auth0Id
                };

                Playlist playlist = new Playlist
                {
                    Name = "DEFAULT",
                    PlaylistId = playlistId,
                    RoomId = roomId,
                    CreatedByUserId = userId,
                    ModifiedByUserId = userId
                };

                Room room = new Room
                {
                    Name = "DEFAULT",
                    RoomId = roomId,
                    PlaylistId = playlistId,
                    CreatedByUserId = userId,
                    ModifiedByUserId = userId
                };

                using var transaction = _videoManagerDbContext.Database.BeginTransaction();
                try
                {
                    await _videoManagerDbContext.Users.AddAsync(user);
                    await _videoManagerDbContext.Playlists.AddAsync(playlist);
                    await _videoManagerDbContext.Rooms.AddAsync(room);

                    await _videoManagerDbContext.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    // TODO: Handle failure
                }
            }

            _memoryCache.Set(auth0Id, user);

            return user;
        }

        public async Task<Guid?> GetUserIdByAuthId(string auth0Id)
        {
            if (string.IsNullOrEmpty(auth0Id)) return null;
            if (_memoryCache.TryGetValue(auth0Id, out User user)) return user.UserId;

            user = await _videoManagerDbContext.Users.FirstOrDefaultAsync(x => x.Auth0Id == auth0Id);

            return user?.UserId;
        }
    }
}
