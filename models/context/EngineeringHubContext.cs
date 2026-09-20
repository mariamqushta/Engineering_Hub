using Engineering_Hub.models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Engineering_Hub.models.context
{
    public class EngineeringHubContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public EngineeringHubContext(DbContextOptions<EngineeringHubContext> options) : base(options)
        {
        }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<Track> Tracks { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<LessonProgress> LessonProgresses { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }

        public DbSet<TrackInstructor> TrackInstructors { get; set; }
        public DbSet<TrackEnrollment> TrackEnrollments { get; set; }

        public DbSet<Workshop> Workshops { get; set; }
        public DbSet<WorkshopBooking> WorkshopBookings { get; set; }

        public DbSet<InteractiveActivity> InteractiveActivities { get; set; }
        public DbSet<InteractiveBooking> InteractiveBookings { get; set; }

        public DbSet<TrackPackage> TrackPackages { get; set; }
        public DbSet<TrackPackageBooking> TrackPackageBookings { get; set; }
        public DbSet<TrackPackageWorkshop> TrackPackageWorkshops { get; set; }

        public DbSet<TrackPackageInteractive> TrackPackageInteractives { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // TrackInstructor composite key
            builder.Entity<TrackInstructor>()
                .HasKey(x => new { x.UserId, x.TrackId });

            builder.Entity<TrackInstructor>()
                .HasOne(x => x.User)
                .WithMany(x => x.TrackInstructors)
                .HasForeignKey(x => x.UserId);

            builder.Entity<TrackInstructor>()
                .HasOne(x => x.Track)
                .WithMany(x => x.TrackInstructors)
                .HasForeignKey(x => x.TrackId);

            // Decimal precision
            builder.Entity<AssignmentSubmission>()
                .Property(x => x.Grade)
                .HasPrecision(5, 2);

            builder.Entity<Track>()
                .Property(x => x.Price)
                .HasPrecision(18, 2);

            builder.Entity<Workshop>()
                .Property(x => x.Price)
                .HasPrecision(18, 2);

            builder.Entity<InteractiveActivity>()
                .Property(x => x.Price)
                .HasPrecision(18, 2);

            builder.Entity<TrackPackage>()
                .Property(x => x.Price)
                .HasPrecision(18, 2);

            // LessonProgress relationships
            builder.Entity<LessonProgress>()
                .HasOne(x => x.Student)
                .WithMany(x => x.LessonProgresses)
                .HasForeignKey(x => x.StudentId);

            builder.Entity<LessonProgress>()
                .HasOne(x => x.Lesson)
                .WithMany(x => x.LessonProgresses)
                .HasForeignKey(x => x.LessonId);

            builder.Entity<LessonProgress>()
                .HasIndex(x => new { x.StudentId, x.LessonId })
                .IsUnique();

            // TrackPackageWorkshop
            builder.Entity<TrackPackageWorkshop>()
                .HasKey(x => new
                {
                    x.TrackPackageId,
                    x.WorkshopId
                });

            builder.Entity<TrackPackageWorkshop>()
               .HasOne(x => x.TrackPackage)
               .WithMany(x => x.TrackPackageWorkshops)
               .HasForeignKey(x => x.TrackPackageId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TrackPackageWorkshop>()
                .HasOne(x => x.Workshop)
                .WithMany()
                .HasForeignKey(x => x.WorkshopId);

            // TrackPackageInteractive
            builder.Entity<TrackPackageInteractive>()
                .HasKey(x => new
                {
                    x.TrackPackageId,
                    x.InteractiveActivityId
                });
            builder.Entity<TrackPackageInteractive>()
                .HasOne(x => x.TrackPackage)
                .WithMany(x => x.TrackPackageInteractives)
                .HasForeignKey(x => x.TrackPackageId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TrackPackageInteractive>()
                .HasOne(x => x.InteractiveActivity)
                .WithMany()
                .HasForeignKey(x => x.InteractiveActivityId);
        }
    }
    }
