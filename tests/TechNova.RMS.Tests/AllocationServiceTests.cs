using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechNova.RMS.ConsoleApp.Application.Repositories;
using TechNova.RMS.ConsoleApp.Application.Services;
using TechNova.RMS.ConsoleApp.Domain;
using Xunit;

namespace TechNova.RMS.Tests;

public class AllocationServiceTests
{
    [Fact]
    public async Task ValidAllocation_Succeeds()
    {
        var emp = new Employee(Guid.NewGuid(), "T", "t@t", "dev", "eng", EmploymentType.FullTime, 50000);
        var proj = new Project(Guid.NewGuid(), "P", "C", DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(10), ProjectStatus.Active);
        var empRepo = new InMemoryEmployeeRepo(new[] { emp });
        var projRepo = new InMemoryProjectRepo(new[] { proj });
        var allocRepo = new InMemoryAllocationRepo();
        var notifier = new DummyNotifier();
        var svc = new AllocationService(allocRepo, empRepo, projRepo, notifier);

        var a = await svc.CreateAsync(emp.Id, proj.Id, proj.StartDate, proj.EndDate, 50);
        Assert.NotNull(a);
        var list = await allocRepo.GetAllAsync();
        Assert.Single(list);
    }

    [Fact]
    public async Task CapacityExactly100_Succeeds()
    {
        var emp = new Employee(Guid.NewGuid(), "T", "t@t", "dev", "eng", EmploymentType.FullTime, 50000);
        var proj = new Project(Guid.NewGuid(), "P", "C", DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(2), ProjectStatus.Active);
        var empRepo = new InMemoryEmployeeRepo(new[] { emp });
        var projRepo = new InMemoryProjectRepo(new[] { proj });
        var allocRepo = new InMemoryAllocationRepo();
        // existing allocation 50%
        var existing = new Allocation(Guid.NewGuid(), emp.Id, proj.Id, proj.StartDate, proj.EndDate, 50);
        await allocRepo.AddAsync(existing);
        var svc = new AllocationService(allocRepo, empRepo, projRepo, new DummyNotifier());

        var a = await svc.CreateAsync(emp.Id, proj.Id, proj.StartDate, proj.EndDate, 50);
        Assert.NotNull(a);
    }

    [Fact]
    public async Task CapacityExceeding100_IsRejected()
    {
        var emp = new Employee(Guid.NewGuid(), "T", "t@t", "dev", "eng", EmploymentType.FullTime, 50000);
        var proj = new Project(Guid.NewGuid(), "P", "C", DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(2), ProjectStatus.Active);
        var empRepo = new InMemoryEmployeeRepo(new[] { emp });
        var projRepo = new InMemoryProjectRepo(new[] { proj });
        var allocRepo = new InMemoryAllocationRepo();
        // existing allocation 60%
        var existing = new Allocation(Guid.NewGuid(), emp.Id, proj.Id, proj.StartDate, proj.EndDate, 60);
        await allocRepo.AddAsync(existing);
        var svc = new AllocationService(allocRepo, empRepo, projRepo, new DummyNotifier());

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await svc.CreateAsync(emp.Id, proj.Id, proj.StartDate, proj.EndDate, 50));
    }

    [Fact]
    public async Task InactiveEmployee_CannotBeAllocated()
    {
        var emp = new Employee(Guid.NewGuid(), "T", "t@t", "dev", "eng", EmploymentType.FullTime, 50000);
        emp.Deactivate();
        var proj = new Project(Guid.NewGuid(), "P", "C", DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(2), ProjectStatus.Active);
        var empRepo = new InMemoryEmployeeRepo(new[] { emp });
        var projRepo = new InMemoryProjectRepo(new[] { proj });
        var allocRepo = new InMemoryAllocationRepo();
        var svc = new AllocationService(allocRepo, empRepo, projRepo, new DummyNotifier());

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await svc.CreateAsync(emp.Id, proj.Id, proj.StartDate, proj.EndDate, 10));
    }

    [Fact]
    public async Task ReleasedAllocations_DoNotConsumeCapacity()
    {
        var emp = new Employee(Guid.NewGuid(), "T", "t@t", "dev", "eng", EmploymentType.FullTime, 50000);
        var proj = new Project(Guid.NewGuid(), "P", "C", DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(2), ProjectStatus.Active);
        var empRepo = new InMemoryEmployeeRepo(new[] { emp });
        var projRepo = new InMemoryProjectRepo(new[] { proj });
        var allocRepo = new InMemoryAllocationRepo();
        var existing = new Allocation(Guid.NewGuid(), emp.Id, proj.Id, proj.StartDate, proj.EndDate, 80);
        existing.Release();
        await allocRepo.AddAsync(existing);
        var svc = new AllocationService(allocRepo, empRepo, projRepo, new DummyNotifier());

        var a = await svc.CreateAsync(emp.Id, proj.Id, proj.StartDate, proj.EndDate, 50);
        Assert.NotNull(a);
    }

    // in-memory test doubles
    private class InMemoryEmployeeRepo : IEmployeeRepository
    {
        private readonly List<Employee> _list;
        public InMemoryEmployeeRepo(IEnumerable<Employee> items) => _list = items.ToList();
        public Task AddAsync(Employee e) { _list.Add(e); return Task.CompletedTask; }
        public Task<IEnumerable<Employee>> GetAllAsync() => Task.FromResult<IEnumerable<Employee>>(_list);
        public Task<Employee?> GetByIdAsync(Guid id) => Task.FromResult(_list.FirstOrDefault(x => x.Id == id));
        public Task UpdateAsync(Employee e)
        {
            var idx = _list.FindIndex(x => x.Id == e.Id);
            if (idx >= 0) _list[idx] = e;
            return Task.CompletedTask;
        }
    }

    private class InMemoryProjectRepo : IProjectRepository
    {
        private readonly List<Project> _list;
        public InMemoryProjectRepo(IEnumerable<Project> items) => _list = items.ToList();
        public Task AddAsync(Project p) { _list.Add(p); return Task.CompletedTask; }
        public Task<IEnumerable<Project>> GetAllAsync() => Task.FromResult<IEnumerable<Project>>(_list);
        public Task<Project?> GetByIdAsync(Guid id) => Task.FromResult(_list.FirstOrDefault(x => x.Id == id));
        public Task UpdateAsync(Project p)
        {
            var idx = _list.FindIndex(x => x.Id == p.Id);
            if (idx >= 0) _list[idx] = p;
            return Task.CompletedTask;
        }
    }

    private class InMemoryAllocationRepo : IAllocationRepository
    {
        private readonly List<Allocation> _list = new();
        public Task AddAsync(Allocation a) { _list.Add(a); return Task.CompletedTask; }
        public Task<IEnumerable<Allocation>> GetAllAsync() => Task.FromResult<IEnumerable<Allocation>>(_list);
        public Task<Allocation?> GetByIdAsync(Guid id) => Task.FromResult(_list.FirstOrDefault(x => x.Id == id));
        public Task UpdateAsync(Allocation a)
        {
            var idx = _list.FindIndex(x => x.Id == a.Id);
            if (idx >= 0) _list[idx] = a;
            return Task.CompletedTask;
        }
    }

    private class DummyNotifier : INotificationService { public Task NotifyAsync(string message) => Task.CompletedTask; }
}
