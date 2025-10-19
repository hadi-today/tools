using System.ComponentModel.DataAnnotations;

namespace Tools.Models;

public class UpdateTaskEstimateRequest
{
    [Required]
    public int TaskId { get; set; }

    [Range(0, 1000, ErrorMessage = "Estimated hours must be between 0 and 1000.")]
    public decimal? EstimatedHours { get; set; }
}
