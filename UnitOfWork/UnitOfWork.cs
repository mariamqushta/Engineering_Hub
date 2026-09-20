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

        GenericRepository<TrackPackage> _trackPackageRepo;
        GenericRepository<TrackPackageBooking> _trackPackageBookingRepo;
        GenericRepository<TrackPackageWorkshop> _trackPackageWorkshopRepo;
        GenericRepository<TrackPackageInteractive> _trackPackageInteractiveRepo;
        GenericRepository<TrackInstructor> _trackInstructorRepo;


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
        public GenericRepository<TrackPackage> TrackPackagerepo {
            get { 
                if (_trackPackageRepo == null) {
                    _trackPackageRepo = new GenericRepository<TrackPackage>(_context);
                } 
                return _trackPackageRepo; 
            } 
        }
        public GenericRepository<TrackPackageBooking> TrackPackageBookingrepo {
            get { 
                if (_trackPackageBookingRepo == null) {
                    _trackPackageBookingRepo = new GenericRepository<TrackPackageBooking>(_context);
                } 
                return _trackPackageBookingRepo; 
            } 
        }
        public GenericRepository<TrackPackageWorkshop> TrackPackageWorkshoprepo { 
            get { 
                if (_trackPackageWorkshopRepo == null) { 
                    _trackPackageWorkshopRepo = new GenericRepository<TrackPackageWorkshop>(_context); 
                } return _trackPackageWorkshopRepo;
            } 
        }
        public GenericRepository<TrackPackageInteractive> TrackPackageInteractiverepo { 
            get {
                if (_trackPackageInteractiveRepo == null) { 
                    _trackPackageInteractiveRepo = new GenericRepository<TrackPackageInteractive>(_context);
                } return _trackPackageInteractiveRepo; 
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
