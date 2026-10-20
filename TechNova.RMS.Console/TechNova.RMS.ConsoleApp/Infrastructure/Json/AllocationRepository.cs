using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechNova.RMS.ConsoleApp.Application.Repositories;
using TechNova.RMS.ConsoleApp.Domain;

namespace TechNova.RMS.ConsoleApp.Infrastructure.Json;

public sealed class AllocationRepository : IAllocationRepository
{
    private const string FileName = "allocations.json";

    public async Task AddAsync(Allocation a)
    {
        var list = await FileStore.LoadAsync<AllocationDto>(FileName);
        list.Add(AllocationDto.From(a));
        await FileStore.SaveAsync(FileName, list);
    }

    public async Task<IEnumerable<Allocation>> GetAllAsync()
    {
        var list = await FileStore.LoadAsync<AllocationDto>(FileName);
        return list.Select(AllocationDto.ToDomain).ToList();
    }

    public async Task<Allocation?> GetByIdAsync(int id)
    {
        var list = await FileStore.LoadAsync<AllocationDto>(FileName);
        var dto = list.FirstOrDefault(x => x.Id == id);
        return dto is null ? null : AllocationDto.ToDomain(dto);
    }

    public async Task UpdateAsync(Allocation a)
    {
        var list = await FileStore.LoadAsync<AllocationDto>(FileName);
        var idx = list.FindIndex(x => x.Id == a.Id);
        if (idx >= 0) list[idx] = AllocationDto.From(a);
        else list.Add(AllocationDto.From(a));
        await FileStore.SaveAsync(FileName, list);
    }

    private record AllocationDto(int Id, int EmployeeId, int ProjectId, DateOnly StartDate, DateOnly EndDate, int Percentage, AllocationStatus Status, DateTime CreatedAt, DateTime? ReleasedAt)
    {
        public static AllocationDto From(Allocation a) => new(a.Id, a.EmployeeId, a.ProjectId, a.StartDate, a.EndDate, a.Percentage, a.Status, a.CreatedAt, a.ReleasedAt);
        public static Allocation ToDomain(AllocationDto d)
        {
            var a = new Allocation(d.Id, d.EmployeeId, d.ProjectId, d.StartDate, d.EndDate, d.Percentage, d.CreatedAt);
            a.RestoreStatus(d.Status, d.ReleasedAt);
            return a;
        }
    }
}
