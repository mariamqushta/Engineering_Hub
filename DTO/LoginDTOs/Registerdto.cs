
using Engineering_Hub.models;
using System.ComponentModel.DataAnnotations;

namespace Engineering_Hub.DTO.LoginDTOs
{
    public class Registerdto
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string FullName { get; set; }

        public string Role { get; set; }
        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public AccountType AccountType { get; set; } = AccountType.GroupStudent;
    }
}
