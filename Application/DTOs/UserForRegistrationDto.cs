﻿using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class UserForRegistrationDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public string? Email { get; set; }

        public string? Password { get; set; }

        public string? ConfirmPassword { get; set; }
    }
}
