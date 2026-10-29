using Engineering_Hub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Engineering_Hub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CalendarController : ControllerBase
    {
        private readonly CalendarService _calendarService;

        public CalendarController(CalendarService calendarService)
        {
            _calendarService = calendarService;
        }
        [HttpGet("instructor/my-calendar")]
        [Authorize(Roles = "Instructor")]
       
        public async Task<IActionResult> GetInstructorCalendar()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var events = await _calendarService
                .GetInstructorCalendarAsync(userId);

            return Ok(events);
        }

        [HttpGet("student/my-calendar")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetStudentCalendar()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var events = await _calendarService
                .GetStudentCalendarAsync(userId);

            return Ok(events);
        }
    }
}