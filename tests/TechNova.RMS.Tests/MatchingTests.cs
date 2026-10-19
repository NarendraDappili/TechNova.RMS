using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechNova.RMS.ConsoleApp.Application.Repositories;
using TechNova.RMS.ConsoleApp.Application.Services.Matching;
using TechNova.RMS.ConsoleApp.Domain;
using Xunit;

namespace TechNova.RMS.Tests;

public class MatchingTests
{
    [Fact]
    public async Task FullSkillMatch_ReturnsCandidate()
    {
        var emp = new Employee(Guid.NewGuid(), "Match", "m@x", "dev", "eng", EmploymentType.FullTime, 70000);
        emp.AddOrUpdateSkill("C#", new Proficiency(5));
        var proj = new Project(Guid.NewGuid(), "P", "C", DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(5), ProjectStatus.Active);
        proj.AddOrUpdateRequirement("C#", new Proficiency(4));

        var empRepo = new InMemoryEmployeeRepo(new[] { emp });
        var projRepo = new InMemoryProjectRepo(new[] { proj });
        var allocRepo = new InMemoryAllocationRepo();

        var matcher = new SimpleMatcher(empRepo, projRepo, allocRepo);
        var results = (await matcher.FindMatchesAsync(proj.Id, proj.StartDate, proj.EndDate)).ToList();
        Assert.Single(results);
        Assert.Empty(results[0].MissingSkills);
    }

    [Fact]
    public async Task InsufficientProficiency_IsMissing()
    {
        var emp = new Employee(Guid.NewGuid(), "Match", "m@x", "dev", "eng", EmploymentType.FullTime, 70000);
        emp.AddOrUpdateSkill("C#", new Proficiency(2));
        var proj = new Project(Guid.NewGuid(), "P", "C", DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(5), ProjectStatus.Active);
        proj.AddOrUpdateRequirement("C#", new Proficiency(4));

        var empRepo = new InMemoryEmployeeRepo(new[] { emp });
        var projRepo = new InMemoryProjectRepo(new[] { proj });
        var allocRepo = new InMemoryAllocationRepo();

        var matcher = new SimpleMatcher(empRepo, projRepo, allocRepo);
        var results = (await matcher.FindMatchesAsync(proj.Id, proj.StartDate, proj.EndDate)).ToList();
        Assert.Single(results);
        Assert.NotEmpty(results[0].MissingSkills);
    }

    // simple in-memory repos
    private class InMemoryEmployeeRepo : IEmployeeRepository
    {
        private readonly List<Employee> _list;
        public InMemoryEmployeeRepo(IEnumerable<Employee> items) => _list = items.ToList();
        public Task AddAsync(Employee e) { _list.Add(e); return Task.CompletedTask; }
        public Task<IEnumerable<Employee>> GetAllAsync() => Task.FromResult<IEnumerable<Employee>>(_list);
        public Task<Employee?> GetByIdAsync(Guid id) => Task.FromResult(_list.FirstOrDefault(x => x.Id == id));
        public Task UpdateAsync(Employee e) { var idx = _list.FindIndex(x => x.Id == e.Id); if (idx>=0) _list[idx]=e; return Task.CompletedTask; }
    }

    private class InMemoryProjectRepo : IProjectRepository
    {
        private readonly List<Project> _list;
        public InMemoryProjectRepo(IEnumerable<Project> items) => _list = items.ToList();
        public Task AddAsync(Project p) { _list.Add(p); return Task.CompletedTask; }
        public Task<IEnumerable<Project>> GetAllAsync() => Task.FromResult<IEnumerable<Project>>(_list);
        public Task<Project?> GetByIdAsync(Guid id) => Task.FromResult(_list.FirstOrDefault(x => x.Id == id));
        public Task UpdateAsync(Project p) { var idx = _list.FindIndex(x => x.Id == p.Id); if (idx>=0) _list[idx]=p; return Task.CompletedTask; }
    }

    private class InMemoryAllocationRepo : IAllocationRepository
    {
        private readonly List<Allocation> _list = new();
        public Task AddAsync(Allocation a) { _list.Add(a); return Task.CompletedTask; }
        public Task<IEnumerable<Allocation>> GetAllAsync() => Task.FromResult<IEnumerable<Allocation>>(_list);
        public Task<Allocation?> GetByIdAsync(Guid id) => Task.FromResult(_list.FirstOrDefault(x => x.Id == id));
        public Task UpdateAsync(Allocation a) { var idx = _list.FindIndex(x => x.Id == a.Id); if (idx>=0) _list[idx]=a; return Task.CompletedTask; }
    }
}
