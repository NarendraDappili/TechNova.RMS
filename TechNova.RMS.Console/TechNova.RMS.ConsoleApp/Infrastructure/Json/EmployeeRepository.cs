using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechNova.RMS.ConsoleApp.Application.Repositories;
using TechNova.RMS.ConsoleApp.Domain;

namespace TechNova.RMS.ConsoleApp.Infrastructure.Json;

public sealed class EmployeeRepository : IEmployeeRepository
{
    private const string FileName = "employees.json";

    public async Task AddAsync(Employee e)
    {
        var list = await FileStore.LoadAsync<EmployeeDto>(FileName);
        list.Add(EmployeeDto.From(e));
        await FileStore.SaveAsync(FileName, list);
    }

    public async Task<IEnumerable<Employee>> GetAllAsync()
    {
        var list = await FileStore.LoadAsync<EmployeeDto>(FileName);
        return list.Select(EmployeeDto.ToDomain).ToList();
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        var list = await FileStore.LoadAsync<EmployeeDto>(FileName);
        var dto = list.FirstOrDefault(x => x.Id == id);
        return dto is null ? null : EmployeeDto.ToDomain(dto);
    }

    public async Task UpdateAsync(Employee e)
    {
        var list = await FileStore.LoadAsync<EmployeeDto>(FileName);
        var idx = list.FindIndex(x => x.Id == e.Id);
        if (idx >= 0) list[idx] = EmployeeDto.From(e);
        else list.Add(EmployeeDto.From(e));
        await FileStore.SaveAsync(FileName, list);
    }

    private record SkillDto(string Name, int Proficiency);

    private record EmployeeDto(int Id, string Name, string Email, string Designation, string Department, EmploymentType EmploymentType, decimal CostRate, bool IsActive, List<SkillDto> Skills)
    {
        public static EmployeeDto From(Employee e) => new(e.Id, e.Name, e.Email, e.Designation, e.Department, e.EmploymentType, e.CostRate, e.IsActive, e.Skills.Select(s => new SkillDto(s.Name, s.Proficiency.Value)).ToList());
        public static Employee ToDomain(EmployeeDto d)
        {
            var e = new Employee(d.Id, d.Name, d.Email, d.Designation, d.Department, d.EmploymentType, d.CostRate);
            if (!d.IsActive) e.Deactivate();
            foreach (var s in d.Skills) e.AddOrUpdateSkill(s.Name, new Proficiency(s.Proficiency));
            return e;
        }
    }
}
