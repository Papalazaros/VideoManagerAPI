using Microsoft.EntityFrameworkCore;
using VideoManager.Models;

namespace VideoManager
{
    public class VideoManagerDbContext : DbContext
    {
        public DbSet<Video> Videos { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Room> Rooms { get; set; }

        public VideoManagerDbContext(DbContextOptions<VideoManagerDbContext> options)
            : base(options)
        { }
    }
}
