using System;
using System.Collections.Generic;

namespace Tools.Models.Core;

public class Task
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal? EstimatedHours { get; set; }

    public int ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    public string? AssignedUserId { get; set; }

    public string? CompletionUrl { get; set; }

    public DateTime? CompletedAt { get; set; }

    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
}
