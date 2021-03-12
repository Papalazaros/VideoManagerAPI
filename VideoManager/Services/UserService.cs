using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VideoManager.Models;

namespace VideoManager.Services
{
    public interface IUserService
    {
        Task<User> CreateOrGetByAuthUser(AuthUser authUser);
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

        public async Task<User> CreateOrGetByAuthUser(AuthUser authUser)
        {
            if (authUser == null) return null;
            if (_memoryCache.TryGetValue(authUser.sub, out User user)) return user;

            user = await _videoManagerDbContext.Users.FirstOrDefaultAsync(x => x.Auth0Id == authUser.sub);

            if (user == null)
            {
                user = new User
                {
                    Auth0Id = authUser.sub,
                    Email = authUser.email.ToLower()
                };

                await _videoManagerDbContext.Users.AddAsync(user);
                await _videoManagerDbContext.SaveChangesAsync();
            }

            _memoryCache.Set(authUser.sub, user);

            return user;
        }
    }
}
