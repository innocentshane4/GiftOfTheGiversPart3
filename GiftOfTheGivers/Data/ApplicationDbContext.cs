using GiftOfTheGivers.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Donor> Donors => Set<Donor>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Admin> Admins => Set<Admin>();
        public DbSet<Volunteer> Volunteers => Set<Volunteer>();
        public DbSet<ReliefProject> ReliefProjects => Set<ReliefProject>();
        public DbSet<ProjectUpdate> ProjectUpdates => Set<ProjectUpdate>();
        public DbSet<Donation> Donations => Set<Donation>();
        public DbSet<DonationSchedule> DonationSchedules => Set<DonationSchedule>();
        public DbSet<TaxCertificate> TaxCertificates => Set<TaxCertificate>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Donor>()
                .HasOne(d => d.User)
                .WithOne(u => u.Donor)
                .HasForeignKey<Donor>(d => d.UserId);

            builder.Entity<Employee>()
                .HasOne(e => e.User)
                .WithOne(u => u.Employee)
                .HasForeignKey<Employee>(e => e.UserId);

            builder.Entity<Admin>()
                .HasOne(a => a.User)
                .WithOne(u => u.Admin)
                .HasForeignKey<Admin>(a => a.UserId);

            builder.Entity<Donation>()
                .HasOne(d => d.Donor)
                .WithMany(donor => donor.Donations)
                .HasForeignKey(d => d.DonorId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Donation>()
                .HasOne(d => d.ReliefProject)
                .WithMany(p => p.Donations)
                .HasForeignKey(d => d.ReliefProjectId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Donation>()
                .HasIndex(d => d.ReferenceNumber)
                .IsUnique();

            builder.Entity<TaxCertificate>()
                .HasOne(t => t.Donation)
                .WithOne(d => d.TaxCertificate)
                .HasForeignKey<TaxCertificate>(t => t.DonationId);

            builder.Entity<TaxCertificate>()
                .HasIndex(t => t.CertificateNumber)
                .IsUnique();

            builder.Entity<ProjectUpdate>()
                .HasOne(pu => pu.ReliefProject)
                .WithMany(p => p.Updates)
                .HasForeignKey(pu => pu.ReliefProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProjectUpdate>()
                .HasOne(pu => pu.Employee)
                .WithMany(e => e.ProjectUpdates)
                .HasForeignKey(pu => pu.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ReliefProject>()
                .HasOne(p => p.CreatedByEmployee)
                .WithMany(e => e.ProjectsCreated)
                .HasForeignKey(p => p.CreatedByEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<DonationSchedule>()
                .HasOne(s => s.Donor)
                .WithMany(d => d.DonationSchedules)
                .HasForeignKey(s => s.DonorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
