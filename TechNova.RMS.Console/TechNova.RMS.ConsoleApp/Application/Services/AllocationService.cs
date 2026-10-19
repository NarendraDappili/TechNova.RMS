using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechNova.RMS.ConsoleApp.Application.Repositories;
using TechNova.RMS.ConsoleApp.Domain;

namespace TechNova.RMS.ConsoleApp.Application.Services;

public sealed class AllocationService : IAllocationService
{
    private readonly IAllocationRepository _allocRepo;
    private readonly IEmployeeRepository _empRepo;
    private readonly IProjectRepository _projRepo;
    private readonly INotificationService _notifier;

    public AllocationService(IAllocationRepository allocRepo, IEmployeeRepository empRepo, IProjectRepository projRepo, INotificationService notifier)
    {
        _allocRepo = allocRepo;
        _empRepo = empRepo;
        _projRepo = projRepo;
        _notifier = notifier;
    }

    public async Task<Allocation> CreateAsync(int employeeId, int projectId, DateOnly start, DateOnly end, int percentage)
    {
        var emp = await _empRepo.GetByIdAsync(employeeId) ?? throw new InvalidOperationException("Employee not found");
        if (!emp.IsActive) throw new InvalidOperationException("Employee is not active");

        var proj = await _projRepo.GetByIdAsync(projectId) ?? throw new InvalidOperationException("Project not found");
        if (start < proj.StartDate || end > proj.EndDate) throw new InvalidOperationException("Allocation must be within project dates");

        // Check capacity for each date in inclusive range
        var existing = (await _allocRepo.GetAllAsync()).Where(a => a.EmployeeId == employeeId && a.Status == AllocationStatus.Active).ToList();

        for (var d = start; d <= end; d = d.AddDays(1))
        {
            var total = percentage + existing.Where(a => a.StartDate <= d && a.EndDate >= d).Sum(a => a.Percentage);
            if (total > 100) throw new InvalidOperationException($"Capacity exceeded on {d}: {total}%");
        }

        var all = (await _allocRepo.GetAllAsync()).ToList();
        var nextId = all.Any() ? all.Max(x => x.Id) + 1 : 1;
        var alloc = new Allocation(nextId, employeeId, projectId, start, end, percentage);
        await _allocRepo.AddAsync(alloc);
        await _notifier.NotifyAsync($"Allocation created: {emp.Name} -> {proj.Name} {start} to {end} ({percentage}%)");
        return alloc;
    }

    public async Task ReleaseAsync(int allocationId)
    {
        var a = await _allocRepo.GetByIdAsync(allocationId) ?? throw new InvalidOperationException("Allocation not found");
        if (a.Status == AllocationStatus.Released) return;
        a.Release();
        await _allocRepo.UpdateAsync(a);
        var emp = await _empRepo.GetByIdAsync(a.EmployeeId);
        var proj = await _projRepo.GetByIdAsync(a.ProjectId);
        await _notifier.NotifyAsync($"Allocation released: {emp?.Name ?? a.EmployeeId.ToString()} from {proj?.Name ?? a.ProjectId.ToString()} ({a.StartDate} to {a.EndDate})");
    }

    public Task<IEnumerable<Allocation>> ListAsync() => _allocRepo.GetAllAsync();
}
