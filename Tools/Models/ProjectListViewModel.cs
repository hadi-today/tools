using System.Collections.Generic;

namespace Tools.Models;

public class ProjectListViewModel
{
    public IList<ProjectListItemViewModel> Projects { get; set; } = new List<ProjectListItemViewModel>();

    public bool HasProjects => Projects.Count > 0;
}

public class ProjectListItemViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int TotalTasks { get; set; }

    public int InProgressTasks { get; set; }

    public int CompletedTasks { get; set; }
}
