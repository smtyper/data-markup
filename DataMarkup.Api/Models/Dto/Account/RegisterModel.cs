﻿using System.ComponentModel.DataAnnotations;

namespace DataMarkup.Api.Models.Dto.Account;

public record RegisterModel
{
    [Required(ErrorMessage = "Username is required.")]
    public string? Username { get; init; }

    [EmailAddress]
    [Required(ErrorMessage = "Email is required.")]
    public string? Email { get; init; }

    [Required(ErrorMessage = "Password is required.")]
    public string? Password { get; init; }
}
