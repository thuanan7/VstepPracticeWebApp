﻿using System.ComponentModel.DataAnnotations;

namespace VstepPractice.API.Models.DTOs.Auth.Requests;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
