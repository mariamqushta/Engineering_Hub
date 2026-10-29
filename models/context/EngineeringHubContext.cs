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
        public DbSet<LessonQuiz> LessonQuizzes { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<QuizOption> QuizOptions { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }
        public DbSet<QuizAnswer> QuizAnswers { get; set; }
        public DbSet<LessonUnlock> LessonUnlocks { get; set; }
        public DbSet<TrackInstructor> TrackInstructors { get; set; }
        public DbSet<TrackEnrollment> TrackEnrollments { get; set; }
        public DbSet<TrackBookingRequest> TrackBookingRequests { get; set; }
        public DbSet<Workshop> Workshops { get; set; }
        public DbSet<WorkshopBooking> WorkshopBookings { get; set; }
        public DbSet<InteractiveSchedule> InteractiveSchedules { get; set; }
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
            builder.Entity<Workshop>()
                    .HasOne(w => w.Instructor)
                    .WithMany()
                    .HasForeignKey(w => w.InstructorId)
                    .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<InteractiveActivity>()
                .HasOne(a => a.Instructor)
                .WithMany()
                .HasForeignKey(a => a.InstructorId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<TrackEnrollment>()
                .HasIndex(x => new { x.StudentId, x.TrackId })
                .IsUnique();
            builder.Entity<TrackBookingRequest>()
                .Property(x => x.RequestedPrice)
                .HasPrecision(18, 2);

            builder.Entity<TrackEnrollment>()
                .Property(x => x.ApprovedPrice)
                .HasPrecision(18, 2);
            builder.Entity<LessonQuiz>()
    .HasOne(q => q.Lesson)
    .WithMany(l => l.Quizzes)
    .HasForeignKey(q => q.LessonId)
    .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<QuizQuestion>()
                .HasOne(q => q.LessonQuiz)
                .WithMany(q => q.Questions)
                .HasForeignKey(q => q.LessonQuizId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<QuizOption>()
                .HasOne(o => o.QuizQuestion)
                .WithMany(q => q.Options)
                .HasForeignKey(o => o.QuizQuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<QuizAttempt>()
                .HasOne(a => a.LessonQuiz)
                .WithMany(q => q.Attempts)
                .HasForeignKey(a => a.LessonQuizId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<QuizAttempt>()
                .HasOne(a => a.Student)
                .WithMany()
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<QuizAnswer>()
                .HasOne(a => a.QuizAttempt)
                .WithMany(a => a.Answers)
                .HasForeignKey(a => a.QuizAttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<QuizAnswer>()
                .HasOne(a => a.QuizQuestion)
                .WithMany()
                .HasForeignKey(a => a.QuizQuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<QuizAnswer>()
                .HasOne(a => a.SelectedOption)
                .WithMany()
                .HasForeignKey(a => a.SelectedOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<QuizAnswer>()
                .HasIndex(a => new { a.QuizAttemptId, a.QuizQuestionId })
                .IsUnique();

            builder.Entity<LessonUnlock>()
                .HasOne(u => u.Student)
                .WithMany()
                .HasForeignKey(u => u.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<LessonUnlock>()
                .HasOne(u => u.Lesson)
                .WithMany(l => l.LessonUnlocks)
                .HasForeignKey(u => u.LessonId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<LessonUnlock>()
                .HasIndex(u => new { u.StudentId, u.LessonId })
                .IsUnique();

            builder.Entity<LessonQuiz>()
                .Property(q => q.PassingScore)
                .HasPrecision(5, 2);

            builder.Entity<QuizAttempt>()
                .Property(a => a.Score)
                .HasPrecision(5, 2);

            builder.Entity<InteractiveSchedule>()
                .HasOne(s => s.Track)
                .WithMany()
                .HasForeignKey(s => s.TrackId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InteractiveSchedule>()
                .HasOne(s => s.Instructor)
                .WithMany()
                .HasForeignKey(s => s.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InteractiveActivity>()
                .HasOne(a => a.InteractiveSchedule)
                .WithMany(s => s.Activities)
                .HasForeignKey(a => a.InteractiveScheduleId)
                .OnDelete(DeleteBehavior.SetNull);
            builder.Entity<InteractiveSchedule>()
            .Property(s => s.Price)
            .HasPrecision(18, 2);
        }
    }
 }
