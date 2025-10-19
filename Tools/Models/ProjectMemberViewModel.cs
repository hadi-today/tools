using System;

namespace Tools.Models;

public class ProjectMemberViewModel
{
    public string UserId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string Role { get; set; } = "Member";

    public bool IsOwner { get; set; }

    public bool IsManager => string.Equals(Role, "Manager", StringComparison.OrdinalIgnoreCase);

    public decimal HourlyRate { get; set; } = ApplicationUser.DefaultHourlyRate;
}
