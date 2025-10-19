using System.Collections.Generic;
using Tools.Models.Core;

namespace Tools.Models;

public class ProjectDetailsViewModel
{
    public Project Project { get; set; } = null!;

    public IReadOnlyList<ProjectTaskViewModel> Tasks { get; set; } = new List<ProjectTaskViewModel>();

    public IReadOnlyList<ProjectMemberViewModel> Members { get; set; } = new List<ProjectMemberViewModel>();

    public bool CanManageMembers { get; set; }

    public bool CanManageAssignments { get; set; }

    public bool CanViewAllTasks { get; set; }

    public bool CanEditEstimates { get; set; }

    public ProjectEstimateSummaryViewModel? EstimateSummary { get; set; }
}
