using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using VideoManager.Models;

namespace VideoManager.Services
{
    public interface IUserService
    {
        Task<User?> CreateOrGetByAuthUser(AuthUser authUser);
    }

    public class UserService : IUserService
    {
        private readonly VideoManagerDbContext _videoManagerDbContext;
        private readonly IMemoryCache _memoryCache;

        public UserService(VideoManagerDbContext videoManagerDbContext,
            IMemoryCache memoryCache)
        {
            _videoManagerDbContext = videoManagerDbContext;
            _memoryCache = memoryCache;
        }

        public async Task<User?> CreateOrGetByAuthUser(AuthUser authUser)
        {
            if (_memoryCache.TryGetValue(authUser.Sub, out User? user))
            {
                return user;
            }

            user = await _videoManagerDbContext.Users.FirstOrDefaultAsync(x => x.Auth0Id == authUser.Sub);

            if (user == null && !string.IsNullOrEmpty(authUser.Sub) && !string.IsNullOrEmpty(authUser.Email))
            {
                user = new User(authUser.Sub, authUser.Email.ToLower());

                await _videoManagerDbContext.Users.AddAsync(user);
                await _videoManagerDbContext.SaveChangesAsync();
            }

            _memoryCache.Set(authUser.Sub, user);

            return user;
        }
    }
}
