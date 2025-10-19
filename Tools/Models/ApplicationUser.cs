using Microsoft.AspNetCore.Identity;

namespace Tools.Models;

public class ApplicationUser : IdentityUser
{
    public const decimal DefaultHourlyRate = 50m;

    public decimal HourlyRate { get; set; } = DefaultHourlyRate;

    public string? OpenAIApiKey { get; set; }

    public string? GeminiApiKey { get; set; }
}
