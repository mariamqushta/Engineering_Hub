using Engineering_Hub.models.context;
using Engineering_Hub.Repository;

namespace Engineering_Hub.UnitOfWork
{
    public class UnitOfWork
    {
        private readonly EngineeringHubContext _context;
 
        public UnitOfWork(EngineeringHubContext context)
        {
            _context = context;

        }
    }
}
