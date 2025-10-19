using System.Collections.Generic;

namespace Tools.Models.Core;

public class Project
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string OwnerId { get; set; } = string.Empty;

    public ICollection<Task> Tasks { get; set; } = new List<Task>();

    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
}
