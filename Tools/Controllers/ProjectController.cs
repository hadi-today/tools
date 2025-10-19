using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Tools.Data;
using Tools.Models;
using Tools.Models.Core;
using Tools.Services;
using ToolTask = Tools.Models.Core.Task;

namespace Tools.Controllers;

public class ProjectController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAiTaskGeneratorService _taskGeneratorService;
    private readonly IProjectEstimateService _estimateService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ProjectController> _logger;

    public ProjectController(
        ApplicationDbContext context,
        IAiTaskGeneratorService taskGeneratorService,
        IProjectEstimateService estimateService,
        UserManager<ApplicationUser> userManager,
        ILogger<ProjectController> logger)
    {
        _context = context;
        _taskGeneratorService = taskGeneratorService;
        _estimateService = estimateService;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var projects = await _context.Projects
            .AsNoTracking()
            .Include(project => project.Tasks)
            .OrderByDescending(project => project.Id)
            .Select(project => new ProjectListItemViewModel
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                TotalTasks = project.Tasks.Count,
                InProgressTasks = project.Tasks.Count(task => task.Status == "In Progress"),
                CompletedTasks = project.Tasks.Count(task => task.Status == "Done")
            })
            .ToListAsync();

        var viewModel = new ProjectListViewModel
        {
            Projects = projects
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> CreateWizard()
    {
        var features = await _context.WebsiteFeatures
            .OrderBy(feature => feature.ParentFeatureId)
            .ThenBy(feature => feature.Title)
            .ToListAsync();

        var viewModel = new CreateWizardViewModel
        {
            AllFeatures = features
        };

        if (TempData.TryGetValue("SelectedFeaturesSummary", out var summary) && summary is string message)
        {
            ViewData["SelectedFeaturesSummary"] = message;
        }

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateProject([FromForm] List<int>? selectedFeatureIds, [FromForm] string? projectName)
    {
        var ids = selectedFeatureIds ?? new List<int>();

        var selectedFeatures = await _context.WebsiteFeatures
            .Where(feature => ids.Contains(feature.Id))
            .OrderBy(feature => feature.ParentFeatureId)
            .ThenBy(feature => feature.Title)
            .ToListAsync();

        var ownerId = User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        var trimmedProjectName = projectName?.Trim();

        var resolvedProjectName = !string.IsNullOrWhiteSpace(trimmedProjectName)
            ? trimmedProjectName!
            : (selectedFeatures.Count > 0
                ? $"New project - {selectedFeatures.First().Title}"
                : "New project");

        var projectDescription = selectedFeatures.Count > 0
            ? string.Join(", ", selectedFeatures.Select(feature => feature.Title))
            : null;

        if (string.IsNullOrWhiteSpace(ownerId))
        {
            TempData["SelectedFeaturesSummary"] = "You need to be signed in before creating a project.";
            return RedirectToAction(nameof(CreateWizard));
        }

        var currentUser = await _userManager.FindByIdAsync(ownerId);

        if (currentUser is null)
        {
            TempData["SelectedFeaturesSummary"] = "We couldn't load your profile. Please sign in again.";
            return RedirectToAction(nameof(CreateWizard));
        }

        IReadOnlyList<ToolTask> generatedTasks;

        try
        {
            generatedTasks = await _taskGeneratorService.GenerateTasksAsync(
                currentUser,
                selectedFeatures,
                resolvedProjectName,
                projectDescription,
                HttpContext.RequestAborted);
        }
        catch (InvalidOperationException ex)
        {
            TempData["SelectedFeaturesSummary"] = ex.Message;
            return RedirectToAction(nameof(CreateWizard));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate tasks with Gemini for user {UserId}", ownerId);
            TempData["SelectedFeaturesSummary"] = "We couldn't generate tasks automatically. Please try again later.";
            return RedirectToAction(nameof(CreateWizard));
        }

        var project = new Project
        {
            Name = resolvedProjectName,
            Description = projectDescription,
            OwnerId = ownerId
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        foreach (var generatedTask in generatedTasks)
        {
            var taskEntity = new ToolTask
            {
                Title = generatedTask.Title,
                Description = generatedTask.Description,
                Status = string.IsNullOrWhiteSpace(generatedTask.Status) ? "To Do" : generatedTask.Status,
                EstimatedHours = generatedTask.EstimatedHours,
                AssignedUserId = generatedTask.AssignedUserId,
                CompletionUrl = generatedTask.CompletionUrl,
                CompletedAt = generatedTask.CompletedAt,
                ProjectId = project.Id
            };

            _context.Tasks.Add(taskEntity);
        }

        await _context.SaveChangesAsync();

        return RedirectToAction("Details", new { id = project.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var project = await _context.Projects
            .AsNoTracking()
            .Include(p => p.Tasks)
                .ThenInclude(task => task.Comments)
                    .ThenInclude(comment => comment.User)
            .Include(p => p.Members)
                .ThenInclude(member => member.User)
            .FirstOrDefaultAsync(project => project.Id == id);

        if (project is null)
        {
            return NotFound();
        }

        var currentUserId = User?.FindFirstValue(ClaimTypes.NameIdentifier);

        var managerUserIds = project.Members
            .Where(member => string.Equals(member.Role, "Manager", StringComparison.OrdinalIgnoreCase))
            .Select(member => member.UserId)
            .ToHashSet(StringComparer.Ordinal);

        var isOwner = !string.IsNullOrEmpty(currentUserId) && string.Equals(project.OwnerId, currentUserId, StringComparison.Ordinal);
        var canManageMembers = !string.IsNullOrEmpty(currentUserId) && (isOwner || managerUserIds.Contains(currentUserId!));

        var ownerUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == project.OwnerId);

        var memberViewModels = project.Members
            .Select(member =>
            {
                var displayName = member.User?.UserName ?? member.User?.Email ?? member.UserId;
                var email = member.User?.Email;
                var role = string.IsNullOrWhiteSpace(member.Role) ? "Member" : member.Role;
                var hourlyRate = member.User?.HourlyRate ?? ApplicationUser.DefaultHourlyRate;

                return new ProjectMemberViewModel
                {
                    UserId = member.UserId,
                    DisplayName = displayName,
                    Email = email,
                    Role = role,
                    IsOwner = string.Equals(member.UserId, project.OwnerId, StringComparison.Ordinal),
                    HourlyRate = hourlyRate
                };
            })
            .OrderByDescending(member => member.IsOwner)
            .ThenByDescending(member => member.IsManager)
            .ThenBy(member => member.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (ownerUser is not null)
        {
            var ownerDisplayName = ownerUser.UserName ?? ownerUser.Email ?? ownerUser.Id;
            var existingOwner = memberViewModels.FirstOrDefault(member => string.Equals(member.UserId, ownerUser.Id, StringComparison.Ordinal));

            if (existingOwner is null)
            {
                memberViewModels.Insert(0, new ProjectMemberViewModel
                {
                    UserId = ownerUser.Id,
                    DisplayName = ownerDisplayName,
                    Email = ownerUser.Email,
                    Role = "Owner",
                    IsOwner = true,
                    HourlyRate = ownerUser.HourlyRate
                });
            }
            else
            {
                existingOwner.DisplayName = existingOwner.DisplayName?.Length > 0 ? existingOwner.DisplayName : ownerDisplayName;
                existingOwner.Email ??= ownerUser.Email;
                existingOwner.Role = "Owner";
                existingOwner.IsOwner = true;
                existingOwner.HourlyRate = ownerUser.HourlyRate;
            }
        }
        else if (!memberViewModels.Any(member => string.Equals(member.UserId, project.OwnerId, StringComparison.Ordinal)))
        {
            memberViewModels.Insert(0, new ProjectMemberViewModel
            {
                UserId = project.OwnerId,
                DisplayName = project.OwnerId,
                Role = "Owner",
                IsOwner = true,
                HourlyRate = ApplicationUser.DefaultHourlyRate
            });
        }

        var userLookup = new Dictionary<string, ProjectMemberViewModel>(StringComparer.Ordinal);
        foreach (var member in memberViewModels)
        {
            userLookup[member.UserId] = member;
        }

        var additionalUserIds = project.Tasks
            .Where(task => !string.IsNullOrWhiteSpace(task.AssignedUserId))
            .Select(task => task.AssignedUserId!)
            .Where(userId => !userLookup.ContainsKey(userId))
            .Distinct()
            .ToList();

        if (additionalUserIds.Count > 0)
        {
            var additionalUsers = await _context.Users
                .AsNoTracking()
                .Where(user => additionalUserIds.Contains(user.Id))
                .ToListAsync();

            foreach (var user in additionalUsers)
            {
                var displayName = user.UserName ?? user.Email ?? user.Id;
                var summary = new ProjectMemberViewModel
                {
                    UserId = user.Id,
                    DisplayName = displayName,
                    Email = user.Email,
                    Role = "Contributor",
                    HourlyRate = user.HourlyRate
                };

                userLookup[user.Id] = summary;
            }
        }

        var canManageAssignments = canManageMembers;
        var canEditEstimates = canManageAssignments;
        var canViewAllTasks = canEditEstimates;

        IEnumerable<ToolTask> visibleTasks;
        if (canViewAllTasks)
        {
            visibleTasks = project.Tasks;
        }
        else if (!string.IsNullOrEmpty(currentUserId))
        {
            visibleTasks = project.Tasks
                .Where(task => string.Equals(task.AssignedUserId, currentUserId, StringComparison.Ordinal));
        }
        else
        {
            visibleTasks = Enumerable.Empty<ToolTask>();
        }

        var tasks = visibleTasks
            .OrderBy(task => task.Title)
            .Select(task =>
            {
                var comments = task.Comments
                    .OrderBy(comment => comment.CreatedAt)
                    .Select(comment => new TaskCommentViewModel
                    {
                        Id = comment.Id,
                        Content = comment.Content,
                        AuthorName = comment.User?.Email ?? comment.User?.UserName ?? comment.UserId,
                        CreatedAt = comment.CreatedAt
                    })
                    .ToList();

                var canComment = !string.IsNullOrEmpty(currentUserId) &&
                    (string.Equals(task.AssignedUserId, currentUserId, StringComparison.Ordinal) ||
                     string.Equals(project.OwnerId, currentUserId, StringComparison.Ordinal) ||
                     managerUserIds.Contains(currentUserId));

                ProjectMemberViewModel? assignedSummary = null;
                if (!string.IsNullOrWhiteSpace(task.AssignedUserId))
                {
                    userLookup.TryGetValue(task.AssignedUserId!, out assignedSummary);
                }

                return new ProjectTaskViewModel
                {
                    Task = task,
                    Comments = comments,
                    CanComment = canComment,
                    AvailableMembers = memberViewModels,
                    CanManageAssignments = canManageAssignments,
                    AssignedUserDisplayName = assignedSummary?.DisplayName,
                    AssignedUserEmail = assignedSummary?.Email,
                    CanEditEstimatedHours = canEditEstimates
                };
            })
            .ToList();

        ProjectEstimateSummaryViewModel? estimateSummary = null;
        if (canEditEstimates)
        {
            estimateSummary = _estimateService.BuildSummary(project, userLookup);
        }

        var viewModel = new ProjectDetailsViewModel
        {
            Project = project,
            Tasks = tasks,
            Members = memberViewModels,
            CanManageMembers = canManageMembers,
            CanManageAssignments = canManageAssignments,
            CanViewAllTasks = canViewAllTasks,
            CanEditEstimates = canEditEstimates,
            EstimateSummary = estimateSummary
        };

        return View(viewModel);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InviteMember([FromForm] string email, [FromForm] int projectId)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("Email is required.");
        }

        var project = await _context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project is null)
        {
            return NotFound();
        }

        var currentUserId = User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(currentUserId))
        {
            return Forbid();
        }

        var isOwner = string.Equals(project.OwnerId, currentUserId, StringComparison.Ordinal);
        var isManager = project.Members.Any(member =>
            string.Equals(member.UserId, currentUserId, StringComparison.Ordinal) &&
            string.Equals(member.Role, "Manager", StringComparison.OrdinalIgnoreCase));

        if (!isOwner && !isManager)
        {
            return Forbid();
        }

        var trimmedEmail = email.Trim();
        var normalizedEmail = trimmedEmail.ToUpperInvariant();

        var existingUser = await _context.Users
            .FirstOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail);

        var changesMade = false;

        if (existingUser is not null)
        {
            var alreadyMember = project.Members
                .Any(member => string.Equals(member.UserId, existingUser.Id, StringComparison.Ordinal));

            if (!alreadyMember)
            {
                var newMember = new ProjectMember
                {
                    ProjectId = project.Id,
                    UserId = existingUser.Id,
                    Role = "Member"
                };

                _context.ProjectMembers.Add(newMember);
                changesMade = true;
            }
        }
        else
        {
            var existingInvitation = await _context.Invitations
                .FirstOrDefaultAsync(invitation =>
                    invitation.ProjectId == project.Id &&
                    !invitation.IsCompleted &&
                    invitation.Email.ToUpper() == normalizedEmail);

            if (existingInvitation is null)
            {
                var invitation = new Invitation
                {
                    Email = trimmedEmail,
                    ProjectId = project.Id,
                    Token = Guid.NewGuid().ToString("N"),
                    IsCompleted = false
                };

                _context.Invitations.Add(invitation);
                changesMade = true;
            }
        }

        if (changesMade)
        {
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Details", new { id = project.Id });
    }
}
