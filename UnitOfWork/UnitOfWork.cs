using Engineering_Hub.models;
using Engineering_Hub.models.context;
using Engineering_Hub.Repository;
using System.Threading.Tasks;

namespace Engineering_Hub.UnitOfWork
{
    public class UnitWork
    {
        private readonly EngineeringHubContext _context;

        GenericRepository<Track> _trackRepo;
        GenericRepository<TrackEnrollment> _trackEnrollmentRepo;
        GenericRepository<Lesson> _lessonRepo;
        GenericRepository<LessonProgress> _lessonProgressRepo;
        GenericRepository<Workshop> _workshopRepo;
        GenericRepository<WorkshopBooking> _workshopBookingRepo;
        GenericRepository<InteractiveActivity> _interactiveActivityRepo;
        GenericRepository<InteractiveBooking> _interactiveBookingRepo;
        GenericRepository<InteractiveSchedule> _interactiveScheduleRepo;
        GenericRepository<TrackBookingRequest> _trackBookingRequestRepo;
        GenericRepository<TrackInstructor> _trackInstructorRepo;
        GenericRepository<LessonQuiz> _lessonQuizRepo;
        GenericRepository<QuizQuestion> _quizQuestionRepo;
        GenericRepository<QuizOption> _quizOptionRepo;
        GenericRepository<QuizAttempt> _quizAttemptRepo;
        GenericRepository<QuizAnswer> _quizAnswerRepo;
        GenericRepository<LessonUnlock> _lessonUnlockRepo;
        GenericRepository<LessonContent> _lessonContentRepo;
        GenericRepository<LessonType> _lessonTypeRepo;
        GenericRepository<Assignment> _assignmentRepo;
        GenericRepository<AssignmentSubmission> _assignmentSubmissionRepo;
        GenericRepository<CoachingConversation> _coachingConversationRepo;
        GenericRepository<CoachingMessage> _coachingMessageRepo;
        private GenericRepository<Certificate> _certificateRepo;
        public UnitWork(EngineeringHubContext context)
        {
            _context = context;
        }

        public GenericRepository<Track> Trackrepo
        {
            get
            {
                if (_trackRepo == null)
                {
                    _trackRepo = new GenericRepository<Track>(_context);
                }

                return _trackRepo;
            }
        }
        public GenericRepository<InteractiveSchedule> InteractiveScheduleRepo
        {
            get
            {
                if (_interactiveScheduleRepo == null)
                {
                    _interactiveScheduleRepo =
                        new GenericRepository<InteractiveSchedule>(_context);
                }

                return _interactiveScheduleRepo;
            }
        }

        public GenericRepository<TrackEnrollment> TrackEnrollmentrepo
        {
            get
            {
                if (_trackEnrollmentRepo == null)
                {
                    _trackEnrollmentRepo =
                        new GenericRepository<TrackEnrollment>(_context);
                }

                return _trackEnrollmentRepo;
            }
        }

        public GenericRepository<Lesson> Lessonrepo
        {
            get
            {
                if (_lessonRepo == null)
                {
                    _lessonRepo = new GenericRepository<Lesson>(_context);
                }

                return _lessonRepo;
            }
        }
        public GenericRepository<LessonProgress> LessonProgressrepo
        {
            get
            {
                if (_lessonProgressRepo == null)
                {
                    _lessonProgressRepo =
                        new GenericRepository<LessonProgress>(_context);
                }

                return _lessonProgressRepo;
            }
        }
        public GenericRepository<Assignment> Assignmentrepo
        {
            get
            {
                if (_assignmentRepo == null)
                {
                    _assignmentRepo =
                        new GenericRepository<Assignment>(_context);
                }

                return _assignmentRepo;
            }
        }

        public GenericRepository<AssignmentSubmission> AssignmentSubmissionrepo
        {
            get
            {
                if (_assignmentSubmissionRepo == null)
                {
                    _assignmentSubmissionRepo =
                        new GenericRepository<AssignmentSubmission>(_context);
                }

                return _assignmentSubmissionRepo;
            }
        }

        public GenericRepository<Workshop> Workshoprepo
        {
            get
            {
                if (_workshopRepo == null)
                {
                    _workshopRepo =
                        new GenericRepository<Workshop>(_context);
                }

                return _workshopRepo;
            }
        }

        public GenericRepository<WorkshopBooking> WorkshopBookingrepo
        {
            get
            {
                if (_workshopBookingRepo == null)
                {
                    _workshopBookingRepo =
                        new GenericRepository<WorkshopBooking>(_context);
                }

                return _workshopBookingRepo;
            }
        }
        public GenericRepository<InteractiveActivity> InteractiveActivityrepo
        {
            get
            {
                if (_interactiveActivityRepo == null)
                {
                    _interactiveActivityRepo =
                        new GenericRepository<InteractiveActivity>(_context);
                }

                return _interactiveActivityRepo;
            }
        }

        public GenericRepository<InteractiveBooking> InteractiveBookingrepo
        {
            get
            {
                if (_interactiveBookingRepo == null)
                {
                    _interactiveBookingRepo =
                        new GenericRepository<InteractiveBooking>(_context);
                }

                return _interactiveBookingRepo;
            }
        }

        public GenericRepository<TrackInstructor> TrackInstructorrepo
        {
            get
            {
                if (_trackInstructorRepo == null)
                {
                    _trackInstructorRepo =
                        new GenericRepository<TrackInstructor>(_context);
                }

                return _trackInstructorRepo;
            }
        }
        public GenericRepository<TrackBookingRequest> TrackBookingRequestrepo
        {
            get
            {
                if (_trackBookingRequestRepo == null)
                {
                    _trackBookingRequestRepo =
                        new GenericRepository<TrackBookingRequest>(_context);
                }

                return _trackBookingRequestRepo;
            }
        }
        public GenericRepository<LessonContent> LessonContentrepo
        {
            get
            {
                if (_lessonContentRepo == null)
                {
                    _lessonContentRepo =
                        new GenericRepository<LessonContent>(_context);
                }

                return _lessonContentRepo;
            }
        }
        public GenericRepository<LessonType> LessonTyperepo
        {
            get
            {
                if (_lessonTypeRepo == null)
                {
                    _lessonTypeRepo =
                        new GenericRepository<LessonType>(_context);
                }

                return _lessonTypeRepo;
            }
        }
        public GenericRepository<LessonQuiz> LessonQuizrepo
        {
            get
            {
                if (_lessonQuizRepo == null)
                {
                    _lessonQuizRepo =
                        new GenericRepository<LessonQuiz>(_context);
                }

                return _lessonQuizRepo;
            }
        }

        public GenericRepository<QuizQuestion> QuizQuestionrepo
        {
            get
            {
                if (_quizQuestionRepo == null)
                {
                    _quizQuestionRepo =
                        new GenericRepository<QuizQuestion>(_context);
                }

                return _quizQuestionRepo;
            }
        }

        public GenericRepository<QuizOption> QuizOptionrepo
        {
            get
            {
                if (_quizOptionRepo == null)
                {
                    _quizOptionRepo =
                        new GenericRepository<QuizOption>(_context);
                }

                return _quizOptionRepo;
            }
        }

        public GenericRepository<QuizAttempt> QuizAttemptrepo
        {
            get
            {
                if (_quizAttemptRepo == null)
                {
                    _quizAttemptRepo =
                        new GenericRepository<QuizAttempt>(_context);
                }

                return _quizAttemptRepo;
            }
        }

        public GenericRepository<QuizAnswer> QuizAnswerrepo
        {
            get
            {
                if (_quizAnswerRepo == null)
                {
                    _quizAnswerRepo =
                        new GenericRepository<QuizAnswer>(_context);
                }

                return _quizAnswerRepo;
            }
        }

        public GenericRepository<LessonUnlock> LessonUnlockrepo
        {
            get
            {
                if (_lessonUnlockRepo == null)
                {
                    _lessonUnlockRepo =
                        new GenericRepository<LessonUnlock>(_context);
                }

                return _lessonUnlockRepo;
            }
        }
        public GenericRepository<CoachingConversation> CoachingConversationrepo
        {
            get
            {
                if (_coachingConversationRepo == null)
                {
                    _coachingConversationRepo =
                        new GenericRepository<CoachingConversation>(_context);
                }

                return _coachingConversationRepo;
            }
        }
        public GenericRepository<CoachingMessage> CoachingMessagerepo
        {
            get
            {
                if (_coachingMessageRepo == null)
                {
                    _coachingMessageRepo =
                        new GenericRepository<CoachingMessage>(_context);
                }

                return _coachingMessageRepo;
            }
        }

        public GenericRepository<Certificate> Certificaterepo
        {
            get
            {
                if (_certificateRepo == null)
                {
                    _certificateRepo =
                        new GenericRepository<Certificate>(_context);
                }

                return _certificateRepo;
            }
        }
        public void DeleteTrackInstructor(TrackInstructor trackInstructor)
        {
            _context.TrackInstructors.Remove(trackInstructor);
        }
        public void Save()
        {
            _context.SaveChanges();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
