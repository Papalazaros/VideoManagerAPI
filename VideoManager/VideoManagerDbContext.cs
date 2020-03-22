using Microsoft.EntityFrameworkCore;
using VideoManager.Models;

namespace VideoManager
{
    public class VideoManagerDbContext : DbContext
    {
        public DbSet<Video> Videos { get; set; }

        public VideoManagerDbContext(DbContextOptions<VideoManagerDbContext> options)
            : base(options)
        { }
    }
}
