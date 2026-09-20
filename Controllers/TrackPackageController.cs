using Engineering_Hub.DTO.BookingDTOs;
using Engineering_Hub.DTO.TrackPackage;
using Engineering_Hub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Engineering_Hub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrackPackageController : ControllerBase
    {
        private readonly ITrackPackageService _packageService;

        public TrackPackageController(
            ITrackPackageService packageService)
        {
            _packageService = packageService;
        }

        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> CreatePackage(
            TrackPackageDTO dto)
        {
            var instructorId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (instructorId == null)
                return Unauthorized();

            var package =
                await _packageService.CreatePackageAsync(
                    dto,
                    instructorId);

            if (package == null)
            {
                return BadRequest(
                    "Track not found, invalid data, or you are not assigned to this Track.");
            }

            return Ok(package);
        }

        [HttpGet("Track/{trackId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPackagesByTrack(
            int trackId)
        {
            var packages =
                await _packageService
                    .GetPackagesByTrackAsync(trackId);

            return Ok(packages);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> UpdatePackage(
            int id,
            TrackPackageDTO dto)
        {
            var instructorId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (instructorId == null)
                return Unauthorized();

            var result =
                await _packageService.UpdatePackageAsync(
                    id,
                    dto,
                    instructorId);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }
    }
}