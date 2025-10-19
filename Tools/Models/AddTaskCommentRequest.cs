using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Tools.Models;

public class AddTaskCommentRequest
{
    [Required]
    [JsonPropertyName("taskId")]
    public int TaskId { get; set; }

    [Required]
    [StringLength(500)]
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}
