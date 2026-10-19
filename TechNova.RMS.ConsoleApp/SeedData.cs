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
            new Employee(Guid.Parse("00000000-0000-0000-0000-000000000001"), "Alice Smith", "alice@acme.com", "Developer", "Engineering", EmploymentType.FullTime, 90000),
            new Employee(Guid.Parse("00000000-0000-0000-0000-000000000002"), "Bob Jones", "bob@acme.com", "Developer", "Engineering", EmploymentType.Contract, 80000),
            new Employee(Guid.Parse("00000000-0000-0000-0000-000000000003"), "Carol White", "carol@acme.com", "QA", "Quality", EmploymentType.FullTime, 70000),
            new Employee(Guid.Parse("00000000-0000-0000-0000-000000000004"), "Dan Brown", "dan@acme.com", "Manager", "Engineering", EmploymentType.FullTime, 110000),
            new Employee(Guid.Parse("00000000-0000-0000-0000-000000000005"), "Eve Black", "eve@client.com", "Consultant", "Services", EmploymentType.Contract, 120000),
            new Employee(Guid.Parse("00000000-0000-0000-0000-000000000006"), "Frank Green", "frank@acme.com", "Developer", "Engineering", EmploymentType.Intern, 30000),
            new Employee(Guid.Parse("00000000-0000-0000-0000-000000000007"), "Grace Hopper", "grace@acme.com", "Architect", "Architecture", EmploymentType.FullTime, 150000),
            new Employee(Guid.Parse("00000000-0000-0000-0000-000000000008"), "Hank Pym", "hank@acme.com", "DevOps", "Operations", EmploymentType.FullTime, 95000),
            new Employee(Guid.Parse("00000000-0000-0000-0000-000000000009"), "Ivy Blue", "ivy@acme.com", "Developer", "Engineering", EmploymentType.Contract, 85000),
            new Employee(Guid.Parse("00000000-0000-0000-0000-000000000010"), "Jack Black", "jack@acme.com", "Designer", "Design", EmploymentType.FullTime, 65000),
            new Employee(Guid.Parse("00000000-0000-0000-0000-000000000011"), "Karen Gray", "karen@acme.com", "Business Analyst", "Business", EmploymentType.FullTime, 88000),
            new Employee(Guid.Parse("00000000-0000-0000-0000-000000000012"), "Leo Long", "leo@acme.com", "Developer", "Engineering", EmploymentType.Contract, 82000),
            new Employee(Guid.Parse("00000000-0000-0000-0000-000000000013"), "Mona Lisa", "mona@acme.com", "QA", "Quality", EmploymentType.FullTime, 72000),
            new Employee(Guid.Parse("00000000-0000-0000-0000-000000000014"), "Ned Stark", "ned@acme.com", "Support", "Support", EmploymentType.FullTime, 60000),
            new Employee(Guid.Parse("00000000-0000-0000-0000-000000000015"), "Olga Roman", "olga@acme.com", "Developer", "Engineering", EmploymentType.FullTime, 97000),
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
        return new List<Project>
        {
            new Project(Guid.Parse("10000000-0000-0000-0000-000000000001"), "Project Apollo", "Acme Corp", today.AddDays(-30), today.AddDays(120), ProjectStatus.Active),
            new Project(Guid.Parse("10000000-0000-0000-0000-000000000002"), "Project Beacon", "Beta LLC", today.AddDays(-10), today.AddDays(60), ProjectStatus.Active),
            new Project(Guid.Parse("10000000-0000-0000-0000-000000000003"), "Project Comet", "Gamma Inc", today.AddDays(10), today.AddDays(200), ProjectStatus.Proposed),
            new Project(Guid.Parse("10000000-0000-0000-0000-000000000004"), "Project Delta", "Delta Co", today.AddDays(-60), today.AddDays(30), ProjectStatus.Active),
            new Project(Guid.Parse("10000000-0000-0000-0000-000000000005"), "Project Echo", "Echo Ltd", today.AddDays(0), today.AddDays(90), ProjectStatus.Active),
        };
    }

    private static List<Allocation> CreateAllocations(List<Employee> emps, List<Project> projs)
    {
        var a = new List<Allocation>
        {
            new Allocation(Guid.NewGuid(), emps[0].Id, projs[0].Id, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-10), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30), 50),
            new Allocation(Guid.NewGuid(), emps[1].Id, projs[0].Id, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-5), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(60), 50),
            new Allocation(Guid.NewGuid(), emps[2].Id, projs[1].Id, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-2), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(20), 100),
            new Allocation(Guid.NewGuid(), emps[3].Id, projs[1].Id, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(10), 20),
            new Allocation(Guid.NewGuid(), emps[4].Id, projs[2].Id, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(5), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(40), 60),
            new Allocation(Guid.NewGuid(), emps[5].Id, projs[3].Id, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-15), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(15), 30),
            new Allocation(Guid.NewGuid(), emps[6].Id, projs[0].Id, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(0), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(90), 20),
            new Allocation(Guid.NewGuid(), emps[7].Id, projs[4].Id, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(0), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30), 80),
            new Allocation(Guid.NewGuid(), emps[8].Id, projs[4].Id, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(10), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(50), 40),
            new Allocation(Guid.NewGuid(), emps[9].Id, projs[3].Id, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-3), DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7), 60),
        };

        return a;
    }
}
