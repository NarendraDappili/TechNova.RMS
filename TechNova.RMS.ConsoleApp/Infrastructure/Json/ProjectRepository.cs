using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechNova.RMS.ConsoleApp.Application.Repositories;
using TechNova.RMS.ConsoleApp.Domain;

namespace TechNova.RMS.ConsoleApp.Infrastructure.Json;

internal sealed class ProjectRepository : IProjectRepository
{
    private const string FileName = "projects.json";

    public async Task AddAsync(Project p)
    {
        var list = await FileStore.LoadAsync<ProjectDto>(FileName);
        list.Add(ProjectDto.From(p));
        await FileStore.SaveAsync(FileName, list);
    }

    public async Task<IEnumerable<Project>> GetAllAsync()
    {
        var list = await FileStore.LoadAsync<ProjectDto>(FileName);
        return list.Select(ProjectDto.ToDomain).ToList();
    }

    public async Task<Project?> GetByIdAsync(Guid id)
    {
        var list = await FileStore.LoadAsync<ProjectDto>(FileName);
        var dto = list.FirstOrDefault(x => x.Id == id);
        return dto is null ? null : ProjectDto.ToDomain(dto);
    }

    public async Task UpdateAsync(Project p)
    {
        var list = await FileStore.LoadAsync<ProjectDto>(FileName);
        var idx = list.FindIndex(x => x.Id == p.Id);
        if (idx >= 0) list[idx] = ProjectDto.From(p);
        else list.Add(ProjectDto.From(p));
        await FileStore.SaveAsync(FileName, list);
    }

    private record RequirementDto(string SkillName, int MinProficiency);

    private record ProjectDto(Guid Id, string Name, string Client, DateOnly StartDate, DateOnly EndDate, ProjectStatus Status, List<RequirementDto> Requirements)
    {
        public static ProjectDto From(Project p) => new(p.Id, p.Name, p.Client, p.StartDate, p.EndDate, p.Status, p.Requirements.Select(r => new RequirementDto(r.SkillName, r.MinProficiency.Value)).ToList());
        public static Project ToDomain(ProjectDto d)
        {
            var p = new Project(d.Id, d.Name, d.Client, d.StartDate, d.EndDate, d.Status);
            foreach (var r in d.Requirements) p.AddOrUpdateRequirement(r.SkillName, new Proficiency(r.MinProficiency));
            return p;
        }
    }
}
