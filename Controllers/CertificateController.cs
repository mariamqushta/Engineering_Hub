using Engineering_Hub.DTO;
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
    public class CertificateController : ControllerBase
    {
        private readonly ICertificateService _certificateService;

        public CertificateController(
            ICertificateService certificateService)
        {
            _certificateService = certificateService;
        }

        [HttpGet("my-certificates")]
        public async Task<IActionResult> GetMyCertificates()
        {
            var studentId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(studentId))
            {
                return Unauthorized();
            }

            var certificates =
                await _certificateService
                    .GetMyCertificatesAsync(studentId);

            return Ok(certificates);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCertificateById(
            int id)
        {
            var studentId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(studentId))
            {
                return Unauthorized();
            }

            var certificate =
                await _certificateService
                    .GetCertificateByIdAsync(
                        id,
                        studentId);

            if (certificate == null)
            {
                return NotFound(
                    "Certificate not found.");
            }

            return Ok(certificate);
        }
    }
}