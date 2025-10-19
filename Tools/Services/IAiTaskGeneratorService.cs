using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tools.Models;
using Tools.Models.Core;
using ToolTask = Tools.Models.Core.Task;

namespace Tools.Services;

public interface IAiTaskGeneratorService
{
    Task<IReadOnlyList<ToolTask>> GenerateTasksAsync(
        ApplicationUser user,
        IReadOnlyCollection<WebsiteFeature> selectedFeatures,
        string projectName,
        string? projectSummary,
        CancellationToken cancellationToken = default);
}
