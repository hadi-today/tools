using System.Collections.Generic;

namespace Tools.Models;

public class ProjectEstimateSummaryViewModel
{
    public decimal TotalEstimatedHours { get; set; }

    public decimal TotalEstimatedCost { get; set; }

    public IReadOnlyList<ProjectMemberEstimateViewModel> MemberSummaries { get; set; } = new List<ProjectMemberEstimateViewModel>();
}

public class ProjectMemberEstimateViewModel
{
    public string UserId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public decimal HourlyRate { get; set; }

    public decimal AssignedHours { get; set; }

    public decimal EstimatedCost { get; set; }

    public bool IsUnassigned => string.IsNullOrEmpty(UserId);
}
