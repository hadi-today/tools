using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tools.Models;
using Tools.Models.Core;
using ToolTask = Tools.Models.Core.Task;

namespace Tools.Services;

public class AiTaskGeneratorService : IAiTaskGeneratorService
{
    private const string GeminiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
    private const string SystemInstruction = """
You are an elite Senior Software Architect and Project Manager. Your mission is to analyze a user's natural language project description and convert it into a detailed, technical execution plan.

Your entire response MUST be a single, valid JSON array of task objects: `[...]`.
ABSOLUTELY NO prose, explanations, or markdown code fences are allowed before or after the JSON array.

Each JSON object in the array must conform to this exact schema:
`{"title": string, "description": string, "estimatedHours": number, "status": "To Do"}`

**Content Generation Rules:**
1.  **Analyze & Deconstruct:** Meticulously analyze the user's project overview to identify all key features, entities, and user stories.
2.  **Granular Breakdown:** Break down large features into specific, manageable tasks. A single task should ideally not exceed 24 hours.
3.  **Full-Stack Coverage:** Ensure tasks cover the database, backend APIs, and frontend UI components.
4.  **Infer Unstated Needs:** Infer and include necessary but unstated tasks, such as initial project setup, authentication, and security measures.
5.  **Strict Formatting Adherence:** The "status" for every task must always be "To Do". The "estimatedHours" must be a numeric value.
""";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<AiTaskGeneratorService> _logger;

    public AiTaskGeneratorService(HttpClient httpClient, ILogger<AiTaskGeneratorService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ToolTask>> GenerateTasksAsync(
        ApplicationUser user,
        IReadOnlyCollection<WebsiteFeature> selectedFeatures,
        string projectName,
        string? projectSummary,
        CancellationToken cancellationToken = default)
    {
        if (user is null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (string.IsNullOrWhiteSpace(user.GeminiApiKey))
        {
            throw new InvalidOperationException("Please add your Google Gemini API key in Settings before generating tasks.");
        }

        var trimmedProjectName = string.IsNullOrWhiteSpace(projectName) ? "New project" : projectName.Trim();
        var trimmedSummary = string.IsNullOrWhiteSpace(projectSummary)
            ? "No detailed project summary was provided."
            : projectSummary.Trim();

        var capabilityLines = selectedFeatures.Count > 0
            ? selectedFeatures
                .Select((feature, index) =>
                {
                    var descriptionSuffix = string.IsNullOrWhiteSpace(feature.Description)
                        ? string.Empty
                        : $" — {feature.Description.Trim()}";

                    return $"{index + 1}. {feature.Title.Trim()}{descriptionSuffix}";
                })
                .ToList()
            : new List<string> { "No specific capabilities were selected. Infer sensible defaults." };

        var userPromptBuilder = new StringBuilder();
        userPromptBuilder
            .AppendLine($"Project name: {trimmedProjectName}")
            .AppendLine($"Project overview: {trimmedSummary}")
            .AppendLine("Selected capabilities:");

        foreach (var line in capabilityLines)
        {
            userPromptBuilder.AppendLine(line);
        }

        userPromptBuilder.AppendLine()
            .AppendLine("Return only the JSON payload described in the system instructions.");

        var payload = new GeminiRequest
        {
            SystemInstruction = new GeminiInstruction
            {
                Parts = new[]
                {
                    new GeminiPart { Text = SystemInstruction }
                }
            },
            Contents = new[]
            {
                new GeminiContent
                {
                    Role = "user",
                    Parts = new[]
                    {
                        new GeminiPart { Text = userPromptBuilder.ToString() }
                    }
                }
            },
            GenerationConfig = new GeminiGenerationConfig
            {
                Temperature = 0.2,
                MaxOutputTokens = 2048,
                ResponseMimeType = "application/json"
            }
        };

        var requestJson = JsonSerializer.Serialize(payload, SerializerOptions);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{GeminiEndpoint}?key={Uri.EscapeDataString(user.GeminiApiKey)}");
        httpRequest.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to contact Gemini for user {UserId}", user.Id);
            throw new InvalidOperationException("We couldn't reach Google Gemini. Please try again later.", ex);
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Gemini returned an error for user {UserId}: {StatusCode} - {Body}",
                user.Id,
                response.StatusCode,
                responseBody);

            throw new InvalidOperationException("Gemini could not generate tasks for this project.");
        }

        string? jsonPlan;
        try
        {
            using var document = JsonDocument.Parse(responseBody);
            jsonPlan = ExtractPlanJson(document.RootElement);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Unable to parse Gemini response JSON for user {UserId}: {Body}", user.Id, responseBody);
            throw new InvalidOperationException("Gemini returned an unexpected response format.", ex);
        }

        var parsedTasks = ParseTasks(jsonPlan);

        if (parsedTasks.Count == 0)
        {
            _logger.LogWarning(
                "Gemini returned no actionable tasks for user {UserId}. Falling back to default template.",
                user.Id);

            return GetFallbackTasks(trimmedProjectName);
        }

        return parsedTasks;
    }

    private IReadOnlyList<ToolTask> ParseTasks(string? jsonPlan)
    {
        if (string.IsNullOrWhiteSpace(jsonPlan))
        {
            return Array.Empty<ToolTask>();
        }

        try
        {
            using var document = JsonDocument.Parse(jsonPlan);

            JsonElement tasksElement;
            switch (document.RootElement.ValueKind)
            {
                case JsonValueKind.Array:
                    tasksElement = document.RootElement;
                    break;
                case JsonValueKind.Object when document.RootElement.TryGetProperty("tasks", out var tasksProperty) && tasksProperty.ValueKind == JsonValueKind.Array:
                    tasksElement = tasksProperty;
                    break;
                default:
                    return Array.Empty<ToolTask>();
            }

            var tasks = new List<ToolTask>();

            foreach (var element in tasksElement.EnumerateArray())
            {
                var title = GetString(element, "title") ?? GetString(element, "name");
                if (string.IsNullOrWhiteSpace(title))
                {
                    continue;
                }

                var description = GetString(element, "description") ?? GetString(element, "details");
                var status = NormalizeStatus(GetString(element, "status"));
                var estimatedHours = GetEstimatedHours(element);

                tasks.Add(new ToolTask
                {
                    Title = title.Trim(),
                    Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                    Status = status,
                    EstimatedHours = estimatedHours,
                    AssignedUserId = null,
                    CompletionUrl = null,
                    CompletedAt = null
                });
            }

            return tasks;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse Gemini task payload: {Payload}", jsonPlan);
            return Array.Empty<ToolTask>();
        }
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.GetRawText(),
            _ => null
        };
    }

    private static decimal? GetEstimatedHours(JsonElement element)
    {
        if (TryGetDecimal(element, "estimated_hours", out var value))
        {
            return value;
        }

        if (TryGetDecimal(element, "estimatedHours", out value))
        {
            return value;
        }

        if (TryGetDecimal(element, "duration_hours", out value))
        {
            return value;
        }

        return null;
    }

    private static bool TryGetDecimal(JsonElement element, string propertyName, out decimal? value)
    {
        value = null;
        if (!element.TryGetProperty(propertyName, out var hoursElement))
        {
            return false;
        }

        switch (hoursElement.ValueKind)
        {
            case JsonValueKind.Number:
                if (hoursElement.TryGetDecimal(out var decimalValue))
                {
                    value = NormalizeHours(decimalValue);
                    return true;
                }

                if (hoursElement.TryGetDouble(out var doubleValue))
                {
                    value = NormalizeHours((decimal)doubleValue);
                    return true;
                }

                break;
            case JsonValueKind.String:
                var raw = hoursElement.GetString();
                if (string.IsNullOrWhiteSpace(raw))
                {
                    break;
                }

                if (decimal.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedDecimal))
                {
                    value = NormalizeHours(parsedDecimal);
                    return true;
                }

                if (decimal.TryParse(raw, NumberStyles.Float, CultureInfo.CurrentCulture, out parsedDecimal))
                {
                    value = NormalizeHours(parsedDecimal);
                    return true;
                }

                break;
        }

        return false;
    }

    private static decimal NormalizeHours(decimal value)
    {
        var clamped = Math.Max(0m, value);
        return Math.Round(clamped, 2, MidpointRounding.AwayFromZero);
    }

    private static string NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return "To Do";
        }

        var normalised = status.Trim().ToLowerInvariant();
        return normalised switch
        {
            "to do" or "todo" or "not started" or "backlog" => "To Do",
            "in progress" or "doing" or "active" => "In Progress",
            "done" or "completed" or "complete" => "Done",
            _ => "To Do"
        };
    }

    private static IReadOnlyList<ToolTask> GetFallbackTasks(string projectName)
    {
        return new List<ToolTask>
        {
            new()
            {
                Title = $"Define scope for {projectName}",
                Description = "Confirm goals, stakeholders, and deliverables to align the team on project expectations.",
                Status = "To Do",
                EstimatedHours = 3m
            },
            new()
            {
                Title = "Draft implementation milestones",
                Description = "Break the project into clear milestones with acceptance criteria and sequencing.",
                Status = "To Do",
                EstimatedHours = 5m
            },
            new()
            {
                Title = "Prepare technical foundation",
                Description = "Set up repositories, environments, and baseline architecture needed for delivery.",
                Status = "To Do",
                EstimatedHours = 4m
            },
            new()
            {
                Title = "Plan QA and release strategy",
                Description = "Outline testing approach, release cadence, and feedback loops for the project.",
                Status = "To Do",
                EstimatedHours = 2m
            }
        };
    }

    private static string? ExtractPlanJson(JsonElement root)
    {
        if (!root.TryGetProperty("candidates", out var candidates) || candidates.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (var candidate in candidates.EnumerateArray())
        {
            if (!candidate.TryGetProperty("content", out var content))
            {
                continue;
            }

            if (!content.TryGetProperty("parts", out var parts) || parts.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var part in parts.EnumerateArray())
            {
                if (part.TryGetProperty("text", out var textElement))
                {
                    var text = textElement.GetString();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return text;
                    }
                }
            }
        }

        return null;
    }

    private sealed class GeminiRequest
    {
        public GeminiInstruction SystemInstruction { get; set; } = new();
        public IReadOnlyList<GeminiContent> Contents { get; set; } = Array.Empty<GeminiContent>();
        public GeminiGenerationConfig GenerationConfig { get; set; } = new();
    }

    private sealed class GeminiInstruction
    {
        public IReadOnlyList<GeminiPart> Parts { get; set; } = Array.Empty<GeminiPart>();
    }

    private sealed class GeminiContent
    {
        public string Role { get; set; } = string.Empty;
        public IReadOnlyList<GeminiPart> Parts { get; set; } = Array.Empty<GeminiPart>();
    }

    private sealed class GeminiPart
    {
        public string Text { get; set; } = string.Empty;
    }

    private sealed class GeminiGenerationConfig
    {
        public double Temperature { get; set; }

        public int MaxOutputTokens { get; set; }

        public string ResponseMimeType { get; set; } = "application/json";
    }
}
