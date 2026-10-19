using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechNova.RMS.ConsoleApp.Application.Repositories;
using TechNova.RMS.ConsoleApp.Application.Services.Matching;
using TechNova.RMS.ConsoleApp.Domain;

namespace TechNova.RMS.ConsoleApp.Application.Services.Matching;

public sealed class SimpleMatcher : IMatcher
{
    private readonly IEmployeeRepository _empRepo;
    private readonly IProjectRepository _projRepo;
    private readonly IAllocationRepository _allocRepo;

    public SimpleMatcher(IEmployeeRepository empRepo, IProjectRepository projRepo, IAllocationRepository allocRepo)
    {
        _empRepo = empRepo;
        _projRepo = projRepo;
        _allocRepo = allocRepo;
    }

    public async Task<IEnumerable<MatchResult>> FindMatchesAsync(int projectId, DateOnly start, DateOnly end)
    {
        var proj = await _projRepo.GetByIdAsync(projectId) ?? throw new InvalidOperationException("Project not found");
        var emps = (await _empRepo.GetAllAsync()).Where(e => e.IsActive).ToList();
        var activeAllocs = (await _allocRepo.GetAllAsync()).Where(a => a.Status == AllocationStatus.Active).ToList();

        var results = new List<MatchResult>();

        foreach (var e in emps)
        {
            // availability check
            var available = true;
            for (var d = start; d <= end; d = d.AddDays(1))
            {
                var total = activeAllocs.Where(a => a.EmployeeId == e.Id && a.StartDate <= d && a.EndDate >= d).Sum(a => a.Percentage);
                if (total >= 100) { available = false; break; }
            }
            if (!available) continue;

            var matched = new List<string>();
            var missing = new List<string>();
            int score = 0;

            if (!proj.Requirements.Any())
            {
                // no requirements: match by availability and cost (lower cost higher score)
                score = 50;
                matched.Add("No skills required");
            }
            else
            {
                foreach (var req in proj.Requirements)
                {
                    var skill = e.Skills.FirstOrDefault(s => string.Equals(s.Name, req.SkillName, StringComparison.OrdinalIgnoreCase));
                    if (skill is null) { missing.Add(req.SkillName); }
                    else
                    {
                        if (skill.Proficiency.Value >= req.MinProficiency.Value)
                        {
                            matched.Add(req.SkillName);
                            score += 10 + (skill.Proficiency.Value - req.MinProficiency.Value);
                        }
                        else
                        {
                            missing.Add(req.SkillName + $" (need {req.MinProficiency.Value})");
                        }
                    }
                }
            }

            // cost penalty
            score -= (int)Math.Round(e.CostRate / 1000);

            results.Add(new MatchResult(e.Id, e.Name, score, matched, missing));
        }

        return results.OrderByDescending(r => r.Score).ThenBy(r => r.EmployeeName).ToList();
    }
}
