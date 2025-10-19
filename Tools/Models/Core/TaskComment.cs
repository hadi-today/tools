using System;
using Tools.Models;

namespace Tools.Models.Core;

public class TaskComment
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public Task Task { get; set; } = null!;

    public ApplicationUser User { get; set; } = null!;
}
