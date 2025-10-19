using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Tools.Models;

public class UpdateTaskStatusRequest
{
    [Required]
    public int TaskId { get; set; }

    [Required]
    [MaxLength(100)]
    [JsonPropertyName("newStatus")]
    public string NewStatus { get; set; } = string.Empty;

    private string? _completionUrl;

    [JsonPropertyName("completionUrl")]
    public string? CompletionUrl
    {
        get => _completionUrl;
        set
        {
            _completionUrl = value;
            HasCompletionUrl = true;
        }
    }

    [JsonIgnore]
    public bool HasCompletionUrl { get; private set; }
}
