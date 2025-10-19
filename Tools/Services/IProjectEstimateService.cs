using System.Collections.Generic;
using System.Threading.Tasks;
using Tools.Models;
using Tools.Models.Core;

namespace Tools.Services;

public interface IProjectEstimateService
{
    ProjectEstimateSummaryViewModel BuildSummary(Project project, IReadOnlyDictionary<string, ProjectMemberViewModel> memberLookup);

    Task<ProjectEstimateSummaryViewModel> CalculateAsync(int projectId);
}
