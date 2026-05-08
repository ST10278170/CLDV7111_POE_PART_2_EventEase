using CLDV7111_POE_PART_1_EventEase.Models;
using Microsoft.EntityFrameworkCore;

namespace CLDV7111_POE_PART_1_EventEase.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Venue> Venues { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // EVENT → BOOKINGS (Cascade allowed)
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Event)
                .WithMany(e => e.Bookings)
                .HasForeignKey(b => b.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // VENUE → BOOKINGS (Restrict to avoid multiple cascade paths)
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Venue)
                .WithMany(v => v.Bookings)
                .HasForeignKey(b => b.VenueId)
                .OnDelete(DeleteBehavior.Restrict);

            // VENUE → EVENTS (Cascade allowed)
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Venue)
                .WithMany(v => v.Events)
                .HasForeignKey(e => e.VenueId)
                .OnDelete(DeleteBehavior.Cascade);

            // Prevent double booking (Venue + Event must be unique)
            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.VenueId, b.EventId })
                .IsUnique();
        }
    }
}
