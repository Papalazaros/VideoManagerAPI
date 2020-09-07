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
        Task<int?> GetUserIdByAuthId(string auth0Id);
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
                using var transaction = _videoManagerDbContext.Database.BeginTransaction();

                try
                {
                    user = new User
                    {
                        Auth0Id = auth0Id,
                        EmailAddress = user.EmailAddress
                    };

                    await _videoManagerDbContext.Users.AddAsync(user);
                    await _videoManagerDbContext.SaveChangesAsync();

                    Playlist playlist = new Playlist
                    {
                        Name = "DEFAULT",
                        CreatedByUser = user,
                        ModifiedByUser = user
                    };

                    await _videoManagerDbContext.Playlists.AddAsync(playlist);
                    await _videoManagerDbContext.SaveChangesAsync();

                    Room room = new Room
                    {
                        Name = "DEFAULT",
                        Playlist = playlist,
                        CreatedByUser = user,
                        ModifiedByUser = user
                    };

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

        public async Task<int?> GetUserIdByAuthId(string auth0Id)
        {
            if (string.IsNullOrEmpty(auth0Id)) return null;
            if (_memoryCache.TryGetValue(auth0Id, out User user)) return user.UserId;

            user = await _videoManagerDbContext.Users.FirstOrDefaultAsync(x => x.Auth0Id == auth0Id);

            return user?.UserId;
        }
    }
}
