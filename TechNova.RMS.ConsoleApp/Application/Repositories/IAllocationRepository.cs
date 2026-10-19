using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechNova.RMS.ConsoleApp.Domain;

namespace TechNova.RMS.ConsoleApp.Application.Repositories;

public interface IAllocationRepository
{
    Task<IEnumerable<Allocation>> GetAllAsync();
    Task<Allocation?> GetByIdAsync(Guid id);
    Task AddAsync(Allocation a);
    Task UpdateAsync(Allocation a);
}
