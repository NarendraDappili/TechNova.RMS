using System;
using System.Linq;
using System.Threading.Tasks;
using TechNova.RMS.ConsoleApp.Application.Repositories;
using TechNova.RMS.ConsoleApp.Domain;

namespace TechNova.RMS.ConsoleApp.Application.Services;

public sealed class ProjectService(IProjectRepository projects) : IProjectService
{
    public async Task<Project> AddAsync(string name, string client, DateOnly startDate, DateOnly endDate, ProjectStatus status)
    {
        var existing = await projects.GetAllAsync();
        var id = existing.Select(project => project.Id).DefaultIfEmpty(0).Max() + 1;
        var project = new Project(id, name, client, startDate, endDate, status);
        await projects.AddAsync(project);
        return project;
    }

    public async Task UpdateAsync(int id, string? name, string? client, DateOnly? startDate, DateOnly? endDate, ProjectStatus? status)
    {
        var project = await GetRequiredAsync(id);
        project.Update(name, client, startDate, endDate, status);
        await projects.UpdateAsync(project);
    }

    public async Task AddOrUpdateRequirementAsync(int projectId, string skillName, int minProficiency)
    {
        var project = await GetRequiredAsync(projectId);
        project.AddOrUpdateRequirement(skillName, new Proficiency(minProficiency));
        await projects.UpdateAsync(project);
    }

    public async Task RemoveRequirementAsync(int projectId, string skillName)
    {
        var project = await GetRequiredAsync(projectId);
        project.RemoveRequirement(skillName);
        await projects.UpdateAsync(project);
    }

    private async Task<Project> GetRequiredAsync(int id) =>
        await projects.GetByIdAsync(id) ?? throw new InvalidOperationException($"Project {id} was not found");
}
