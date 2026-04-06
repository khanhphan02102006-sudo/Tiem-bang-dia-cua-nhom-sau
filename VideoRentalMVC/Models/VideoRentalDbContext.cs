using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace VideoRentalMVC.Models
{
    public class VideoRentalDbContext : IdentityDbContext<IdentityUser>
    {
        public VideoRentalDbContext(DbContextOptions<VideoRentalDbContext> options)
            : base(options) { }

        public DbSet<Phim> Phims { get; set; }
        public DbSet<Bang> Bangs { get; set; }
        public DbSet<Khach> Khachs { get; set; }
        public DbSet<Thue> Thues { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Phim>()
                .Property(p => p.GiaVon)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Thue>()
                .Property(t => t.PhiTreHan)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Thue>()
                .Property(t => t.TongTien)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Khach>()
                .HasIndex(k => k.IdentityUserId)
                .IsUnique()
                .HasFilter("[IdentityUserId] IS NOT NULL");

            modelBuilder.Entity<Khach>()
                .HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(k => k.IdentityUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Bang>()
                .HasOne(b => b.Phim)
                .WithMany(p => p.Bangs)
                .HasForeignKey(b => b.MaPhim)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Thue>()
                .HasOne(t => t.MaBangNavigation)
                .WithMany(b => b.Thues)
                .HasForeignKey(t => t.MaBang)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Thue>()
                .HasOne(t => t.MaKhachNavigation)
                .WithMany(k => k.Thues)
                .HasForeignKey(t => t.MaKhach)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
