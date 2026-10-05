using Engineering_Hub.models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

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
        public DbSet<LessonContent> LessonContents { get; set; }
        public DbSet<LessonType> LessonTypes { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }

        public DbSet<TrackInstructor> TrackInstructors { get; set; }
        public DbSet<TrackEnrollment> TrackEnrollments { get; set; }

        public DbSet<Workshop> Workshops { get; set; }
        public DbSet<WorkshopBooking> WorkshopBookings { get; set; }

        public DbSet<InteractiveActivity> InteractiveActivities { get; set; }
        public DbSet<InteractiveBooking> InteractiveBookings { get; set; }
        public DbSet<CoachingConversation> CoachingConversations { get; set; }
        public DbSet<CoachingMessage> CoachingMessages { get; set; }
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
            // TrackEnrollment
            builder.Entity<TrackEnrollment>()
                .HasIndex(x => new { x.StudentId, x.TrackId })
                .IsUnique();

        
            // =========================
            // Lesson Content
            // =========================

            builder.Entity<LessonContent>()
                .HasOne(x => x.Lesson)
                .WithMany(x => x.LessonContents)
                .HasForeignKey(x => x.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<LessonContent>()
                .HasOne(x => x.LessonType)
                .WithMany(x => x.LessonContents)
                .HasForeignKey(x => x.LessonTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<LessonContent>()
                .HasOne(x => x.Workshop)
                .WithMany()
                .HasForeignKey(x => x.WorkshopId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<LessonContent>()
                .HasOne(x => x.InteractiveActivity)
                .WithMany()
                .HasForeignKey(x => x.InteractiveActivityId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<LessonType>().HasData(
                   new LessonType { Id = 1, Name = "Video" },
                   new LessonType { Id = 2, Name = "PDF" },
                   new LessonType { Id = 3, Name = "PowerPoint" },
                   new LessonType { Id = 4, Name = "Interactive" },
                   new LessonType { Id = 5, Name = "Workshop" } );

            builder.Entity<CoachingConversation>()
            .HasOne(c => c.Student)
            .WithMany()
            .HasForeignKey(c => c.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CoachingConversation>()
                .HasOne(c => c.Track)
                .WithMany()
                .HasForeignKey(c => c.TrackId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CoachingMessage>()
                .HasOne(m => m.Conversation)
                .WithMany()
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CoachingMessage>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
 }
