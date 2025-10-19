using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Tools.Data;
using Tools.Models;
using Tools.Models.Core;
using Tools.Services;

namespace Tools.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class TaskController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IProjectEstimateService _estimateService;
    private static readonly string[] TaskStatuses = new[] { "To Do", "In Progress", "Done" };

    public TaskController(ApplicationDbContext context, IProjectEstimateService estimateService)
    {
        _context = context;
        _estimateService = estimateService;
    }

    [HttpPost("update-status")]
    public async Task<IActionResult> UpdateTaskStatus([FromBody] UpdateTaskStatusRequest request)
    {
        if (request is null)
        {
            return BadRequest(new { error = "Invalid request payload." });
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var normalizedStatus = TaskStatuses
            .FirstOrDefault(status => string.Equals(status, request.NewStatus, StringComparison.OrdinalIgnoreCase));

        if (normalizedStatus is null)
        {
            return BadRequest(new { error = "Invalid task status." });
        }

        var task = await _context.Tasks.FirstOrDefaultAsync(task => task.Id == request.TaskId);

        if (task is null)
        {
            return NotFound();
        }

        var hasChanges = false;

        if (!string.Equals(task.Status, normalizedStatus, StringComparison.Ordinal))
        {
            task.Status = normalizedStatus;
            hasChanges = true;
        }

        if (request.HasCompletionUrl)
        {
            var trimmedUrl = string.IsNullOrWhiteSpace(request.CompletionUrl)
                ? null
                : request.CompletionUrl!.Trim();

            if (!string.Equals(task.CompletionUrl, trimmedUrl, StringComparison.Ordinal))
            {
                task.CompletionUrl = trimmedUrl;
                hasChanges = true;
            }
        }

        if (hasChanges)
        {
            await _context.SaveChangesAsync();
        }

        return Ok(new { success = true, status = normalizedStatus, completionUrl = task.CompletionUrl });
    }

    [HttpPost("update-estimate")]
    [Authorize]
    public async Task<IActionResult> UpdateEstimate([FromBody] UpdateTaskEstimateRequest request)
    {
        if (request is null)
        {
            return BadRequest(new { error = "Invalid request payload." });
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized();
        }

        var task = await _context.Tasks
            .Include(task => task.Project)
                .ThenInclude(project => project.Members)
            .FirstOrDefaultAsync(task => task.Id == request.TaskId);

        if (task is null)
        {
            return NotFound();
        }

        var isProjectOwner = string.Equals(task.Project.OwnerId, currentUserId, StringComparison.Ordinal);
        var isProjectManager = task.Project.Members.Any(member =>
            string.Equals(member.UserId, currentUserId, StringComparison.Ordinal) &&
            !string.IsNullOrEmpty(member.Role) &&
            string.Equals(member.Role, "Manager", StringComparison.OrdinalIgnoreCase));

        if (!isProjectOwner && !isProjectManager)
        {
            return Forbid();
        }

        decimal? normalizedEstimate = null;
        if (request.EstimatedHours.HasValue)
        {
            if (request.EstimatedHours.Value < 0)
            {
                return BadRequest(new { error = "Estimated hours must be zero or greater." });
            }

            normalizedEstimate = decimal.Round(request.EstimatedHours.Value, 2, MidpointRounding.AwayFromZero);
        }

        if (task.EstimatedHours != normalizedEstimate)
        {
            task.EstimatedHours = normalizedEstimate;
            await _context.SaveChangesAsync();
        }

        var summary = await _estimateService.CalculateAsync(task.ProjectId);

        return Ok(new
        {
            success = true,
            estimatedHours = task.EstimatedHours,
            summary = new
            {
                totalHours = summary.TotalEstimatedHours,
                totalCost = summary.TotalEstimatedCost,
                members = summary.MemberSummaries.Select(member => new
                {
                    userId = member.UserId,
                    displayName = member.DisplayName,
                    hourlyRate = member.HourlyRate,
                    assignedHours = member.AssignedHours,
                    estimatedCost = member.EstimatedCost,
                    isUnassigned = member.IsUnassigned
                })
            }
        });
    }

    [HttpPost("add-comment")]
    [Authorize]
    public async Task<IActionResult> AddComment([FromBody] AddTaskCommentRequest request)
    {
        if (request is null)
        {
            return BadRequest(new { error = "Invalid request payload." });
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var content = request.Content?.Trim();

        if (string.IsNullOrWhiteSpace(content))
        {
            return BadRequest(new { error = "Comment content cannot be empty." });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var task = await _context.Tasks
            .Include(task => task.Project)
            .FirstOrDefaultAsync(task => task.Id == request.TaskId);

        if (task is null)
        {
            return NotFound();
        }

        var isAssignedUser = string.Equals(task.AssignedUserId, userId, StringComparison.Ordinal);
        var isProjectOwner = string.Equals(task.Project.OwnerId, userId, StringComparison.Ordinal);
        var isProjectManager = await _context.ProjectMembers
            .AnyAsync(member => member.ProjectId == task.ProjectId
                && member.UserId == userId
                && member.Role != null
                && member.Role.ToUpper() == "MANAGER");

        if (!isAssignedUser && !isProjectOwner && !isProjectManager)
        {
            return Forbid();
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        var comment = new TaskComment
        {
            TaskId = task.Id,
            UserId = userId,
            Content = content!,
            CreatedAt = DateTime.UtcNow
        };

        _context.TaskComments.Add(comment);
        await _context.SaveChangesAsync();

        var authorName = user?.Email ?? user?.UserName ?? userId;

        return Ok(new
        {
            success = true,
            comment = new
            {
                id = comment.Id,
                content = comment.Content,
                authorName,
                createdAt = comment.CreatedAt
            }
        });
    }

    [HttpPost("assign-user")]
    [Authorize]
    public async Task<IActionResult> AssignUser([FromBody] AssignTaskMemberRequest request)
    {
        if (request is null)
        {
            return BadRequest(new { error = "Invalid request payload." });
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized();
        }

        var task = await _context.Tasks
            .Include(task => task.Project)
                .ThenInclude(project => project.Members)
            .FirstOrDefaultAsync(task => task.Id == request.TaskId);

        if (task is null)
        {
            return NotFound();
        }

        var isProjectOwner = string.Equals(task.Project.OwnerId, currentUserId, StringComparison.Ordinal);
        var isProjectManager = await _context.ProjectMembers
            .AnyAsync(member => member.ProjectId == task.ProjectId
                && member.UserId == currentUserId
                && member.Role != null
                && member.Role.ToUpper() == "MANAGER");

        if (!isProjectOwner && !isProjectManager)
        {
            return Forbid();
        }

        var trimmedUserId = request.UserId?.Trim();

        if (string.IsNullOrEmpty(trimmedUserId))
        {
            if (string.IsNullOrEmpty(task.AssignedUserId))
            {
                return Ok(new { success = true, assignedUser = (object?)null });
            }

            task.AssignedUserId = null;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, assignedUser = (object?)null });
        }

        var permittedUserIds = new HashSet<string>(StringComparer.Ordinal)
        {
            task.Project.OwnerId
        };

        foreach (var member in task.Project.Members)
        {
            permittedUserIds.Add(member.UserId);
        }

        if (!permittedUserIds.Contains(trimmedUserId))
        {
            return BadRequest(new { error = "Selected user must belong to the project." });
        }

        if (!string.Equals(task.AssignedUserId, trimmedUserId, StringComparison.Ordinal))
        {
            task.AssignedUserId = trimmedUserId;
            await _context.SaveChangesAsync();
        }

        var assignedUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == trimmedUserId);

        var displayName = assignedUser?.UserName ?? assignedUser?.Email ?? trimmedUserId;
        var email = assignedUser?.Email;

        return Ok(new
        {
            success = true,
            assignedUser = new
            {
                id = trimmedUserId,
                displayName,
                email
            }
        });
    }
}
