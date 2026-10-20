using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechNova.RMS.ConsoleApp.Application.Repositories;
using TechNova.RMS.ConsoleApp.Domain;

namespace TechNova.RMS.ConsoleApp;

internal static class SeedData
{
    public static async Task EnsureSeedAsync(IEmployeeRepository empRepo, IProjectRepository projRepo, IAllocationRepository allocRepo)
    {
        var emps = (await empRepo.GetAllAsync()).ToList();
        if (!emps.Any())
        {
            var seeded = CreateEmployees();
            foreach (var e in seeded) await empRepo.AddAsync(e);
            emps = seeded;
        }

        var projs = (await projRepo.GetAllAsync()).ToList();
        if (!projs.Any())
        {
            var seeded = CreateProjects();
            foreach (var p in seeded) await projRepo.AddAsync(p);
            projs = seeded;
        }

        var allocs = (await allocRepo.GetAllAsync()).ToList();
        if (!allocs.Any())
        {
            var seeded = CreateAllocations(emps, projs);
            foreach (var a in seeded) await allocRepo.AddAsync(a);
        }
    }

    private static List<Employee> CreateEmployees()
    {
            var list = new List<Employee>
        {
            new Employee(1, "Alice Smith", "alice@acme.com", "Developer", "Engineering", EmploymentType.FullTime, 90000),
            new Employee(2, "Bob Jones", "bob@acme.com", "Developer", "Engineering", EmploymentType.Contract, 80000),
            new Employee(3, "Carol White", "carol@acme.com", "QA", "Quality", EmploymentType.FullTime, 70000),
            new Employee(4, "Dan Brown", "dan@acme.com", "Manager", "Engineering", EmploymentType.FullTime, 110000),
            new Employee(5, "Eve Black", "eve@client.com", "Consultant", "Services", EmploymentType.Contract, 120000),
            new Employee(6, "Frank Green", "frank@acme.com", "Developer", "Engineering", EmploymentType.Intern, 30000),
            new Employee(7, "Grace Hopper", "grace@acme.com", "Architect", "Architecture", EmploymentType.FullTime, 150000),
            new Employee(8, "Hank Pym", "hank@acme.com", "DevOps", "Operations", EmploymentType.FullTime, 95000),
            new Employee(9, "Ivy Blue", "ivy@acme.com", "Developer", "Engineering", EmploymentType.Contract, 85000),
            new Employee(10, "Jack Black", "jack@acme.com", "Designer", "Design", EmploymentType.FullTime, 65000),
            new Employee(11, "Karen Gray", "karen@acme.com", "Business Analyst", "Business", EmploymentType.FullTime, 88000),
            new Employee(12, "Leo Long", "leo@acme.com", "Developer", "Engineering", EmploymentType.Contract, 82000),
            new Employee(13, "Mona Lisa", "mona@acme.com", "QA", "Quality", EmploymentType.FullTime, 72000),
            new Employee(14, "Ned Stark", "ned@acme.com", "Support", "Support", EmploymentType.FullTime, 60000),
            new Employee(15, "Olga Roman", "olga@acme.com", "Developer", "Engineering", EmploymentType.FullTime, 97000),
        };

        // add some skills
        list[0].AddOrUpdateSkill("C#", new Proficiency(5));
        list[0].AddOrUpdateSkill("SQL", new Proficiency(4));
        list[1].AddOrUpdateSkill("C#", new Proficiency(4));
        list[2].AddOrUpdateSkill("Testing", new Proficiency(5));
        list[3].AddOrUpdateSkill("Leadership", new Proficiency(5));
        list[4].AddOrUpdateSkill("Consulting", new Proficiency(5));
        list[5].AddOrUpdateSkill("C#", new Proficiency(2));
        list[6].AddOrUpdateSkill("Architecture", new Proficiency(5));
        list[7].AddOrUpdateSkill("Azure", new Proficiency(4));
        list[8].AddOrUpdateSkill("C#", new Proficiency(3));
        list[9].AddOrUpdateSkill("UX", new Proficiency(4));
        list[10].AddOrUpdateSkill("Analysis", new Proficiency(4));
        list[11].AddOrUpdateSkill("C#", new Proficiency(4));
        list[12].AddOrUpdateSkill("Testing", new Proficiency(4));
        list[13].AddOrUpdateSkill("Support", new Proficiency(5));
        list[14].AddOrUpdateSkill("C#", new Proficiency(5));

        return list;
    }

    private static List<Project> CreateProjects()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var projects = new List<Project>
        {
            new Project(1, "Project Apollo", "Acme Corp", today.AddDays(-30), today.AddDays(120), ProjectStatus.Active),
            new Project(2, "Project Beacon", "Beta LLC", today.AddDays(-10), today.AddDays(60), ProjectStatus.Active),
            new Project(3, "Project Comet", "Gamma Inc", today.AddDays(10), today.AddDays(200), ProjectStatus.Proposed),
            new Project(4, "Project Delta", "Delta Co", today.AddDays(-60), today.AddDays(30), ProjectStatus.Active),
            new Project(5, "Project Echo", "Echo Ltd", today.AddDays(0), today.AddDays(90), ProjectStatus.Active),
        };

        projects[0].AddOrUpdateRequirement("C#", new Proficiency(4));
        projects[0].AddOrUpdateRequirement("SQL", new Proficiency(3));
        projects[1].AddOrUpdateRequirement("Testing", new Proficiency(4));
        projects[2].AddOrUpdateRequirement("Consulting", new Proficiency(4));
        projects[3].AddOrUpdateRequirement("C#", new Proficiency(3));
        projects[4].AddOrUpdateRequirement("Azure", new Proficiency(3));
        return projects;
    }

    private static List<Allocation> CreateAllocations(List<Employee> emps, List<Project> projs)
    {
        var a = new List<Allocation>
        {
            new Allocation(1, emps[0].Id, projs[0].Id, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-10), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30), 50),
            new Allocation(2, emps[1].Id, projs[0].Id, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-5), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(60), 50),
            new Allocation(3, emps[2].Id, projs[1].Id, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-2), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(20), 100),
            new Allocation(4, emps[3].Id, projs[1].Id, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(10), 20),
            new Allocation(5, emps[4].Id, projs[2].Id, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(5), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(40), 60),
            new Allocation(6, emps[5].Id, projs[3].Id, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-15), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(15), 30),
            new Allocation(7, emps[6].Id, projs[0].Id, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(0), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(90), 20),
            new Allocation(8, emps[7].Id, projs[4].Id, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(0), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30), 80),
            new Allocation(9, emps[8].Id, projs[4].Id, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(10), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(50), 40),
            new Allocation(10, emps[9].Id, projs[3].Id, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-3), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7), 60),
        };

        return a;
    }
}
