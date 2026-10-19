using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechNova.RMS.ConsoleApp.Domain;

namespace TechNova.RMS.ConsoleApp.Application.Services;

public interface IAllocationService
{
    Task<Allocation> CreateAsync(int employeeId, int projectId, DateOnly start, DateOnly end, int percentage);
    Task ReleaseAsync(int allocationId);
    Task<IEnumerable<Allocation>> ListAsync();
}
