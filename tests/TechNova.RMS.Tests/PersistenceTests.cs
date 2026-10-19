using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TechNova.RMS.ConsoleApp.Infrastructure.Json;
using TechNova.RMS.ConsoleApp.Domain;
using TechNova.RMS.ConsoleApp.Application.Repositories;
using Xunit;

namespace TechNova.RMS.Tests;

public class PersistenceTests
{
    [Fact]
    public async Task SaveAndReload_PreservesData()
    {
        var temp = Path.Combine(Path.GetTempPath(), "technova_rms_test", Guid.NewGuid().ToString());
        Environment.SetEnvironmentVariable("TECHNOVA_RMS_DATA_DIR", temp);
        try
        {
            Directory.CreateDirectory(temp);
            var empRepo = new TechNova.RMS.ConsoleApp.Infrastructure.Json.EmployeeRepository();
            var projRepo = new TechNova.RMS.ConsoleApp.Infrastructure.Json.ProjectRepository();
            var allocRepo = new TechNova.RMS.ConsoleApp.Infrastructure.Json.AllocationRepository();

            var emp = new Employee(Guid.NewGuid(), "Persist", "p@x", "dev", "eng", EmploymentType.FullTime, 50000);
            emp.AddOrUpdateSkill("C#", new Proficiency(5));
            await empRepo.AddAsync(emp);

            var proj = new Project(Guid.NewGuid(), "P", "C", DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(5), ProjectStatus.Active);
            proj.AddOrUpdateRequirement("C#", new Proficiency(4));
            await projRepo.AddAsync(proj);

            var alloc = new Allocation(Guid.NewGuid(), emp.Id, proj.Id, proj.StartDate, proj.EndDate, 50);
            await allocRepo.AddAsync(alloc);

            // reload from new repo instances
            var empRepo2 = new TechNova.RMS.ConsoleApp.Infrastructure.Json.EmployeeRepository();
            var projRepo2 = new TechNova.RMS.ConsoleApp.Infrastructure.Json.ProjectRepository();
            var allocRepo2 = new TechNova.RMS.ConsoleApp.Infrastructure.Json.AllocationRepository();

            var emps = (await empRepo2.GetAllAsync()).ToList();
            var projs = (await projRepo2.GetAllAsync()).ToList();
            var allocs = (await allocRepo2.GetAllAsync()).ToList();

            Assert.Contains(emps, e => e.Id == emp.Id && e.Skills.Any(s => s.Name.Equals("C#", StringComparison.OrdinalIgnoreCase)));
            Assert.Contains(projs, p => p.Id == proj.Id && p.Requirements.Any(r => r.SkillName.Equals("C#", StringComparison.OrdinalIgnoreCase)));
            Assert.Contains(allocs, a => a.Id == alloc.Id && a.EmployeeId == emp.Id && a.ProjectId == proj.Id);
        }
        finally
        {
            try { Directory.Delete(temp, true); } catch { }
            Environment.SetEnvironmentVariable("TECHNOVA_RMS_DATA_DIR", null);
        }
    }
}
