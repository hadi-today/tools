using System.ComponentModel.DataAnnotations;

namespace Tools.Models;

public class AssignTaskMemberRequest
{
    [Required]
    public int TaskId { get; set; }

    public string? UserId { get; set; }
}
