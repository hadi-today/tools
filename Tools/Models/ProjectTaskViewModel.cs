using System.Collections.Generic;
using Tools.Models.Core;

namespace Tools.Models;

public class ProjectTaskViewModel
{
    public Models.Core.Task Task { get; set; } = null!;

    public IReadOnlyList<TaskCommentViewModel> Comments { get; set; } = new List<TaskCommentViewModel>();

    public bool CanComment { get; set; }

    public IReadOnlyList<ProjectMemberViewModel> AvailableMembers { get; set; } = new List<ProjectMemberViewModel>();

    public bool CanManageAssignments { get; set; }

    public string? AssignedUserDisplayName { get; set; }

    public string? AssignedUserEmail { get; set; }

    public bool CanEditEstimatedHours { get; set; }
}
