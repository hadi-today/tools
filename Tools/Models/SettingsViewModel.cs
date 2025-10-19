using System.ComponentModel.DataAnnotations;

namespace Tools.Models;

public class SettingsViewModel
{
    [Display(Name = "Hourly rate"), Range(0, 100000, ErrorMessage = "Please enter a value between {1} and {2}.")]
    public decimal HourlyRate { get; set; } = ApplicationUser.DefaultHourlyRate;

    [Display(Name = "OpenAI API key")]
    public string? OpenAIApiKey { get; set; }

    [Display(Name = "Google Gemini API key")]
    public string? GeminiApiKey { get; set; }
}
