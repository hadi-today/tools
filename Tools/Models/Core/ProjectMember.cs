using Tools.Models;

namespace Tools.Models.Core;

public class ProjectMember
{
    public int ProjectId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public Project Project { get; set; } = null!;

    public ApplicationUser? User { get; set; }
}
