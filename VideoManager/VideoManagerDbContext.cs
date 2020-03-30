using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VideoManager.Models;

namespace VideoManager
{
    public class VideoManagerDbContext : DbContext
    {
        public DbSet<Video> Videos { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<PlaylistVideo> PlaylistVideos { get; set; }

        private readonly IHttpContextAccessor _httpContextAccessor;

        public VideoManagerDbContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public VideoManagerDbContext(DbContextOptions<VideoManagerDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override int SaveChanges()
        {
            AddTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            AddTimestamps();
            return await base.SaveChangesAsync();
        }

        private void AddTimestamps()
        {
            var entities = ChangeTracker.Entries().Where(x => x.Entity is BaseEntity
                && (x.State == EntityState.Added || x.State == EntityState.Modified));

            Guid? userId = (Guid?)_httpContextAccessor.HttpContext?.Items["UserId"];

            DateTime utcNow = DateTime.UtcNow;

            foreach (var entity in entities)
            {
                if (entity.State == EntityState.Added)
                {
                    ((BaseEntity)entity.Entity).CreatedDate = utcNow;
                    ((BaseEntity)entity.Entity).CreatedByUserId = userId;
                }

                ((BaseEntity)entity.Entity).ModifiedDate = utcNow;
                ((BaseEntity)entity.Entity).ModifiedByUserId = userId ?? ((BaseEntity)entity.Entity).ModifiedByUserId;
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           modelBuilder.Entity<PlaylistVideo>()
                .HasKey(x => new { x.PlaylistId, x.VideoId });

            modelBuilder.Entity<PlaylistVideo>()
                .HasOne(pt => pt.Video)
                .WithMany(p => p.PlaylistVideos)
                .HasForeignKey(pt => pt.VideoId);

            modelBuilder.Entity<PlaylistVideo>()
                .HasOne(pt => pt.Playlist)
                .WithMany(t => t.PlaylistVideos)
                .HasForeignKey(pt => pt.PlaylistId);

            modelBuilder.Entity<Room>()
                        .HasOne(a => a.Playlist)
                        .WithOne(a => a.Room)
                        .HasForeignKey<Playlist>(c => c.RoomId);
        }
    }
}
