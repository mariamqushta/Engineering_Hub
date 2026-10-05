using AutoMapper;
using Engineering_Hub.DTO;
using Engineering_Hub.UnitOfWork;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public class CertificateService : ICertificateService
    {
        private readonly UnitWork _unitOfWork;
        private readonly IMapper _mapper;

        public CertificateService(
            UnitWork unitWork,
            IMapper mapper)
        {
            _unitOfWork = unitWork;
            _mapper = mapper;
        }

        public async Task<List<CertificateResponseDTO>>
            GetMyCertificatesAsync(
                string studentId)
        {
            var certificates =
                _unitOfWork.Certificaterepo
                    .GetByCondition(c =>
                        c.StudentId == studentId);

            return _mapper.Map<
                List<CertificateResponseDTO>>(
                certificates);
        }

        public async Task<CertificateResponseDTO?>
            GetCertificateByIdAsync(
                int certificateId,
                string studentId)
        {
            var certificate =
                _unitOfWork.Certificaterepo
                    .GetByCondition(c =>
                        c.Id == certificateId &&
                        c.StudentId == studentId)
                    .FirstOrDefault();

            if (certificate == null)
            {
                return null;
            }

            return _mapper.Map<CertificateResponseDTO>(
                certificate);
        }
    }
}