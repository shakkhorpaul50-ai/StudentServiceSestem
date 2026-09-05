using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using IUBAT_Student_Service.Models;
#nullable disable

namespace IUBAT_Student_Service.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ServiceRequest> ServiceRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(entity =>
            {
                // Unique filtered index: no two students can share the same StudentId
                // Staff accounts have null StudentId and are excluded from uniqueness
                entity.HasIndex(e => e.StudentId)
                    .IsUnique()
                    .HasFilter("\"StudentId\" IS NOT NULL");
            });

            builder.Entity<ServiceRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Student)
                    .WithMany()
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
