using Microsoft.EntityFrameworkCore;
using VideoManager.Models;

namespace VideoManager
{
    public class VideoManagerDbContext : DbContext
    {
        public DbSet<Video> Videos { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomVideo> RoomVideos { get; set; }

        public VideoManagerDbContext() { }

        public VideoManagerDbContext(DbContextOptions<VideoManagerDbContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           modelBuilder.Entity<RoomVideo>()
            .HasKey(x => new { x.RoomId, x.VideoId });

            modelBuilder.Entity<RoomVideo>()
                .HasOne(pt => pt.Video)
                .WithMany(p => p.RoomVideos)
                .HasForeignKey(pt => pt.VideoId);

            modelBuilder.Entity<RoomVideo>()
                .HasOne(pt => pt.Room)
                .WithMany(t => t.RoomVideos)
                .HasForeignKey(pt => pt.RoomId);
        }
    }
}
