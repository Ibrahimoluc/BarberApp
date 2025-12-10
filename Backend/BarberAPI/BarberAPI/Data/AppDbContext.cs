using BarberAPI.Models.Concrete;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace BarberAPI.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<AppUser> Users { get; set; }      // Ortak havuz
        public DbSet<Barber> Barbers { get; set; }     // Hizmet verenler
        public DbSet<Customer> Customers { get; set; } // Hizmet alanlar
        public DbSet<BarberServiceArea> BarberServiceAreas { get; set; } // Berberin hizmet verdiği bölgeler

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                var connectionString = configuration.GetConnectionString("Default");

                optionsBuilder
                    .UseNpgsql(connectionString)
                    .LogTo(Console.WriteLine, LogLevel.Information) // Logları konsola bas
                    .EnableSensitiveDataLogging(); // Hata anındaki veriyi göster (Geliştirme aşamasında kullanılır)
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // One-to-One İlişkileri Tanımlıyoruz

            // Bir User'ın bir Barber profili olabilir
            builder.Entity<AppUser>()
                .HasOne(u => u.BarberProfile)
                .WithOne(b => b.AppUser)
                .HasForeignKey<Barber>(b => b.AppUserId);

            // Bir User'ın bir Customer profili olabilir
            builder.Entity<AppUser>()
                .HasOne(u => u.CustomerProfile)
                .WithOne(c => c.AppUser)
                .HasForeignKey<Customer>(c => c.AppUserId);

            builder.Entity<Barber>()
                .HasMany(b => b.ServiceAreas)
                .WithOne(sa => sa.Barber)
                .HasForeignKey(sa => sa.BarberId)
                .OnDelete(DeleteBehavior.Cascade); // ÖNEMLİ: Berber silinirse bölgeleri de sil!
        }
    }
}
