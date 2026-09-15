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

            builder.Entity<TrackPackage>()
                .Property(x => x.Price)
                .HasPrecision(18, 2);
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
        }
    }
    }
