
using Engineering_Hub.DTO;
using Engineering_Hub.models;
using Engineering_Hub.Services;
using Engineering_Hub.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
namespace Engineering_Hub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CertificateController : ControllerBase
    {
        private readonly ICertificateService _certificateService;
        private readonly ITrackCompletionService _trackCompletionService;
        private readonly IWebHostEnvironment _environment;
        private readonly ICertificatePdfService _certificatePdfService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UnitWork _unitOfWork;
        public CertificateController(
            ICertificateService certificateService,
            ITrackCompletionService trackCompletionService,
            IWebHostEnvironment environment,
            ICertificatePdfService certificatePdfService,
            UserManager<ApplicationUser> userManager,
            UnitWork unitOfWork)
        {
            _certificateService = certificateService;
            _trackCompletionService = trackCompletionService;
            _environment = environment;
            _certificatePdfService = certificatePdfService;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        [HttpGet("my-certificates")]
        public async Task<IActionResult> GetMyCertificates()
        {
            var studentId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(studentId))
                return Unauthorized();

            var certificates = await _certificateService
                .GetMyCertificatesAsync(studentId);

            return Ok(certificates);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCertificateById(int id)
        {
            var studentId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(studentId))
                return Unauthorized();

            var certificate = await _certificateService
                .GetCertificateByIdAsync(id, studentId);

            if (certificate == null)
                return NotFound("Certificate not found.");

            return Ok(certificate);
        }

        [HttpGet("track/{trackId}/completion-status")]
        public async Task<IActionResult> GetTrackCompletionStatus(
            int trackId)
        {
            var studentId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(studentId))
                return Unauthorized();

            if (trackId <= 0)
                return BadRequest("Invalid track ID.");

            var isCompleted = await _trackCompletionService
                .IsTrackCompletedAsync(trackId, studentId);

            return Ok(new { isCompleted });
        }



        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadCertificate(int id)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(studentId))
                return Unauthorized();

            // Verify that this certificate belongs to the logged-in student.
            var certificate = await _certificateService
                .GetCertificateByIdAsync(id, studentId);

            if (certificate == null)
                return NotFound("Certificate not found.");

            // Build the expected PDF path.
            var certificatesDirectory = Path.Combine(
                _environment.ContentRootPath,
                "App_Data",
                "certificates");

            Directory.CreateDirectory(certificatesDirectory);

            var pdfPath = Path.Combine(
                certificatesDirectory,
                $"certificate-{id}.pdf");

            // Generate the PDF if it is missing.
            if (!System.IO.File.Exists(pdfPath))
            {
                var student = await _userManager.FindByIdAsync(studentId);
                var track = _unitOfWork.Trackrepo.GetById(certificate.TrackId);

                if (student == null || track == null)
                    return NotFound("Student or track not found.");

                var certificateModel = new Certificate
                {
                    Id = certificate.Id,
                    StudentId = certificate.StudentId,
                    TrackId = certificate.TrackId,
                    IssuedAt = certificate.IssuedAt,
                    CertificateUrl = certificate.CertificateUrl,
                    Student = student,
                    Track = track
                };

                var pdfBytes = _certificatePdfService.Generate(certificateModel);

                await System.IO.File.WriteAllBytesAsync(pdfPath, pdfBytes);
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(pdfPath);

            return File(
                fileBytes,
                "application/pdf",
                $"EngineeringHub-Certificate-{id}.pdf");
        }

    }
}
