using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tools.Data;
using Tools.Models;
using Tools.Models.Core;

namespace Tools.Services;

public class ProjectEstimateService : IProjectEstimateService
{
    private readonly ApplicationDbContext _context;

    public ProjectEstimateService(ApplicationDbContext context)
    {
        _context = context;
    }

    public ProjectEstimateSummaryViewModel BuildSummary(Project project, IReadOnlyDictionary<string, ProjectMemberViewModel> memberLookup)
    {
        if (project is null)
        {
            throw new ArgumentNullException(nameof(project));
        }

        if (memberLookup is null)
        {
            throw new ArgumentNullException(nameof(memberLookup));
        }

        var items = project.Tasks
            .Where(task => task.EstimatedHours.HasValue)
            .Select(task => new EstimateItem(task.AssignedUserId, task.EstimatedHours!.Value))
            .ToList();

        return BuildSummary(items, userId =>
        {
            return memberLookup.TryGetValue(userId, out var member) ? member : null;
        });
    }

    public async Task<ProjectEstimateSummaryViewModel> CalculateAsync(int projectId)
    {
        var items = await _context.Tasks
            .AsNoTracking()
            .Where(task => task.ProjectId == projectId && task.EstimatedHours.HasValue)
            .Select(task => new EstimateItem(task.AssignedUserId, task.EstimatedHours!.Value))
            .ToListAsync();

        if (items.Count == 0)
        {
            return new ProjectEstimateSummaryViewModel();
        }

        var userIds = items
            .Select(item => item.AssignedUserId)
            .Where(userId => !string.IsNullOrEmpty(userId))
            .Distinct()
            .Select(userId => userId!)
            .ToList();

        var users = await _context.Users
            .AsNoTracking()
            .Where(user => userIds.Contains(user.Id))
            .Select(user => new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.HourlyRate
            })
            .ToListAsync();

        var lookup = users.ToDictionary(
            user => user.Id,
            user => new ProjectMemberViewModel
            {
                UserId = user.Id,
                DisplayName = user.UserName ?? user.Email ?? user.Id,
                Email = user.Email,
                Role = "Member",
                HourlyRate = user.HourlyRate
            },
            StringComparer.Ordinal);

        return BuildSummary(items, userId =>
        {
            return lookup.TryGetValue(userId, out var member) ? member : null;
        });
    }

    private static ProjectEstimateSummaryViewModel BuildSummary(IEnumerable<EstimateItem> items, Func<string, ProjectMemberViewModel?> resolveMember)
    {
        var memberSummaries = new List<ProjectMemberEstimateViewModel>();
        var groupedItems = items
            .GroupBy(item => item.AssignedUserId ?? string.Empty, StringComparer.Ordinal);

        decimal totalHours = 0m;
        decimal totalCost = 0m;

        foreach (var group in groupedItems)
        {
            var hours = group.Sum(item => item.Hours);
            var normalizedHours = decimal.Round(hours, 2, MidpointRounding.AwayFromZero);

            if (normalizedHours <= 0)
            {
                continue;
            }

            totalHours += normalizedHours;

        if (string.IsNullOrEmpty(group.Key))
        {
            var hourlyRatetemp = ApplicationUser.DefaultHourlyRate;
            var costtemp = decimal.Round(normalizedHours * hourlyRatetemp, 2, MidpointRounding.AwayFromZero);

            totalCost += costtemp;

            memberSummaries.Add(new ProjectMemberEstimateViewModel
            {
                UserId = string.Empty,
                DisplayName = "Unassigned",
                HourlyRate = hourlyRatetemp,
                AssignedHours = normalizedHours,
                EstimatedCost = costtemp
            });

            continue;
        }

        var member = resolveMember(group.Key);
        var hourlyRate = member?.HourlyRate ?? ApplicationUser.DefaultHourlyRate;
        var displayName = member?.DisplayName ?? group.Key;
        var cost = decimal.Round(normalizedHours * hourlyRate, 2, MidpointRounding.AwayFromZero);

        totalCost += cost;

            memberSummaries.Add(new ProjectMemberEstimateViewModel
            {
                UserId = group.Key,
                DisplayName = displayName,
                HourlyRate = hourlyRate,
                AssignedHours = normalizedHours,
                EstimatedCost = cost
            });
        }

        memberSummaries = memberSummaries
            .OrderBy(summary => summary.IsUnassigned)
            .ThenByDescending(summary => summary.EstimatedCost)
            .ThenByDescending(summary => summary.AssignedHours)
            .ThenBy(summary => summary.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new ProjectEstimateSummaryViewModel
        {
            TotalEstimatedHours = decimal.Round(totalHours, 2, MidpointRounding.AwayFromZero),
            TotalEstimatedCost = decimal.Round(totalCost, 2, MidpointRounding.AwayFromZero),
            MemberSummaries = memberSummaries
        };
    }

    private readonly struct EstimateItem
    {
        public EstimateItem(string? assignedUserId, decimal hours)
        {
            AssignedUserId = assignedUserId;
            Hours = hours;
        }

        public string? AssignedUserId { get; }

        public decimal Hours { get; }
    }
}
