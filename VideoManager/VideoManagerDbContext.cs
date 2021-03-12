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
        public DbSet<Video> Videos => Set<Video>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<RoomVideo> RoomVideos => Set<RoomVideo>();
        public DbSet<RoomMember> RoomMembers => Set<RoomMember>();

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
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void AddTimestamps()
        {
            var entities = ChangeTracker.Entries().Where(x => x.Entity is BaseEntity
                && (x.State == EntityState.Added || x.State == EntityState.Modified));

            int? userId = (int?)_httpContextAccessor.HttpContext?.Items["UserId"];

            DateTime utcNow = DateTime.UtcNow;

            foreach (var entity in entities)
            {
                if (entity.State == EntityState.Added)
                {
                    ((BaseEntity)entity.Entity).CreatedDate = utcNow;
                    ((BaseEntity)entity.Entity).CreatedByUserId ??= userId;
                }

                ((BaseEntity)entity.Entity).ModifiedDate = utcNow;
                ((BaseEntity)entity.Entity).ModifiedByUserId = userId ?? ((BaseEntity)entity.Entity).ModifiedByUserId;
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RoomVideo>()
                 .HasKey(x => new { x.RoomId, x.VideoId });

            modelBuilder.Entity<RoomMember>()
                 .HasKey(x => new { x.RoomId, x.UserId });
        }
    }
}
