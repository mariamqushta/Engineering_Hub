using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Engineering_Hub.DTO.AccountDTOs
{
    public class UpdateProfileDto
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string UserName { get; set; } = string.Empty;

        public IFormFile? ProfileImage { get; set; }
    }
}