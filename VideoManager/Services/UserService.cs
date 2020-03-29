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
        Task<User> CreateOrGetUser(string auth0Id);
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

        public async Task<User> CreateOrGetUser(string auth0Id)
        {
            if (string.IsNullOrEmpty(auth0Id)) return null;
            if (_memoryCache.TryGetValue(auth0Id, out User user)) return user;

            user = await _videoManagerDbContext.Users.FirstOrDefaultAsync(x => x.Auth0Id == auth0Id);

            if (user == null)
            {
                user = new User
                {
                    UserId = Guid.NewGuid(),
                    Auth0Id = auth0Id
                };

                await _videoManagerDbContext.Users.AddAsync(user);
                await _videoManagerDbContext.SaveChangesAsync();
            }

            _memoryCache.Set(auth0Id, user);

            return user;
        }
    }
}
