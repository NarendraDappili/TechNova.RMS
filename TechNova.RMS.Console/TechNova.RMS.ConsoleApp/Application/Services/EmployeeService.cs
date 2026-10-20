using System;
using System.Linq;
using System.Threading.Tasks;
using TechNova.RMS.ConsoleApp.Application.Repositories;
using TechNova.RMS.ConsoleApp.Domain;

namespace TechNova.RMS.ConsoleApp.Application.Services;

public sealed class EmployeeService(IEmployeeRepository employees) : IEmployeeService
{
    public async Task<Employee> AddAsync(string name, string email, string designation, string department, EmploymentType employmentType, decimal costRate)
    {
        var existing = await employees.GetAllAsync();
        var id = existing.Select(employee => employee.Id).DefaultIfEmpty(0).Max() + 1;
        var employee = new Employee(id, name, email, designation, department, employmentType, costRate);
        await employees.AddAsync(employee);
        return employee;
    }

    public async Task UpdateAsync(int id, string? name, string? email, string? designation, string? department, EmploymentType? employmentType, decimal? costRate)
    {
        var employee = await GetRequiredAsync(id);
        employee.UpdateDetails(name, email, designation, department, employmentType, costRate);
        await employees.UpdateAsync(employee);
    }

    public async Task DeactivateAsync(int id)
    {
        var employee = await GetRequiredAsync(id);
        employee.Deactivate();
        await employees.UpdateAsync(employee);
    }

    public async Task AddOrUpdateSkillAsync(int employeeId, string skillName, int proficiency)
    {
        var employee = await GetRequiredAsync(employeeId);
        employee.AddOrUpdateSkill(skillName, new Proficiency(proficiency));
        await employees.UpdateAsync(employee);
    }

    public async Task RemoveSkillAsync(int employeeId, string skillName)
    {
        var employee = await GetRequiredAsync(employeeId);
        employee.RemoveSkill(skillName);
        await employees.UpdateAsync(employee);
    }

    private async Task<Employee> GetRequiredAsync(int id) =>
        await employees.GetByIdAsync(id) ?? throw new InvalidOperationException($"Employee {id} was not found");
}
