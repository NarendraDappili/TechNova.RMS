using System.Collections.Generic;
using System.Threading.Tasks;
using TechNova.RMS.ConsoleApp.Domain;

namespace TechNova.RMS.ConsoleApp.Application.Services;

public interface IEmployeeService
{
    Task<Employee> AddAsync(string name, string email, string designation, string department, EmploymentType employmentType, decimal costRate);
    Task UpdateAsync(int id, string? name, string? email, string? designation, string? department, EmploymentType? employmentType, decimal? costRate);
    Task DeactivateAsync(int id);
    Task AddOrUpdateSkillAsync(int employeeId, string skillName, int proficiency);
    Task RemoveSkillAsync(int employeeId, string skillName);
}
