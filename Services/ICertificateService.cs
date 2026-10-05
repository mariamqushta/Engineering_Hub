using Engineering_Hub.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public interface ICertificateService
    {
        Task<List<CertificateResponseDTO>> GetMyCertificatesAsync(
            string studentId);

        Task<CertificateResponseDTO?> GetCertificateByIdAsync(
            int certificateId,
            string studentId);
    }
}