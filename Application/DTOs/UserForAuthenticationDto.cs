using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class UserForAuthenticationDto
    {
        public string? Email { get; set; }

        public string? Password { get; set; }
    }
}
