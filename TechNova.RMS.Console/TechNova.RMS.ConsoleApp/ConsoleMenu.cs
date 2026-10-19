using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TechNova.RMS.ConsoleApp.Application.Repositories;
using TechNova.RMS.ConsoleApp.Application.Services;
using TechNova.RMS.ConsoleApp.Application.Services.Matching;

namespace TechNova.RMS.ConsoleApp;

public sealed class ConsoleMenu
{
    private readonly IEmployeeRepository _empRepo;
    private readonly IProjectRepository _projRepo;
    private readonly IAllocationService _allocSvc;
    private readonly IAllocationRepository _allocRepo;
    private readonly IMatcher _matcher;

    public ConsoleMenu(IEmployeeRepository empRepo, IProjectRepository projRepo, IAllocationService allocSvc, IAllocationRepository allocRepo, IMatcher matcher)
    {
        _empRepo = empRepo;
        _projRepo = projRepo;
        _allocSvc = allocSvc;
        _allocRepo = allocRepo;
        _matcher = matcher;
    }

    // common input helpers
    private string ReadRequired(string prompt)
    {
        string input;
        do
        {
            Console.Write(prompt);
            input = Console.ReadLine() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(input)) Console.WriteLine("This field is required.");
        } while (string.IsNullOrWhiteSpace(input));
        return input.Trim();
    }

    private Guid ReadGuid(string prompt)
    {
        Guid id;
        do
        {
            Console.Write(prompt);
            var s = Console.ReadLine();
            if (Guid.TryParse(s, out id)) return id;
            Console.WriteLine("Invalid id format. Please enter a valid GUID.");
        } while (true);
    }

    private DateOnly ReadDate(string prompt)
    {
        DateOnly d;
        do
        {
            Console.Write(prompt);
            if (DateOnly.TryParse(Console.ReadLine(), out d)) return d;
            Console.WriteLine("Invalid date. Use format yyyy-MM-dd.");
        } while (true);
    }

    private int ReadInt(string prompt, int min = int.MinValue, int max = int.MaxValue)
    {
        int v;
        do
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out v) && v >= min && v <= max) return v;
            Console.WriteLine("Invalid number.");
        } while (true);
    }

    public async Task RunAsync()
    {
        // ensure seed
        await SeedData.EnsureSeedAsync(_empRepo, _projRepo, _allocRepo);

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("TechNova RMS - Main Menu");
            Console.WriteLine("1. Employee management");
            Console.WriteLine("2. Project management");
            Console.WriteLine("3. Allocation and release");
            Console.WriteLine("4. Employee matching");
            Console.WriteLine("5. Bench report");
            Console.WriteLine("6. Project staffing summary");
            Console.WriteLine("7. Department utilization");
            Console.WriteLine("9. Exit");
            Console.Write("Choice: ");
            var c = Console.ReadLine()?.Trim();
            switch (c)
            {
                case "1": await EmployeeMenu(); break;
                case "2": await ProjectMenu(); break;
                case "3": await AllocationMenu(); break;
                case "4": await MatchingMenu(); break;
                case "5": await BenchReport(); break;
                case "6": await ProjectStaffing(); break;
                case "7": await DepartmentUtilization(); break;
                case "9": return;
                default: Console.WriteLine("Invalid choice"); break;
            }
        }
    }

    private async Task EmployeeMenu()
    {
        Console.WriteLine("Employee Management");
        var emps = (await _empRepo.GetAllAsync()).ToList();
        foreach (var e in emps) Console.WriteLine($"{e.Id} {e.Name} {(e.IsActive?"Active":"Inactive")}");
        Console.WriteLine("a) Add employee, d) Deactivate, b) Back");
        var k = Console.ReadLine();
        if (k == "a")
        {
            // helper to require non-empty input for required fields
            string ReadRequired(string prompt)
            {
                string input;
                do
                {
                    Console.Write(prompt);
                    input = Console.ReadLine() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(input)) Console.WriteLine("This field is required.");
                } while (string.IsNullOrWhiteSpace(input));
                return input.Trim();
            }

            var name = ReadRequired("Name: ");
            var email = ReadRequired("Email: ");
            var des = ReadRequired("Designation: ");
            var dept = ReadRequired("Department: ");
            Console.Write("EmploymentType (FullTime/Contract/Intern): "); var et = Console.ReadLine() ?? "FullTime";
            Console.Write("CostRate: "); var cr = decimal.TryParse(Console.ReadLine(), out var v) ? v : 50000m;
            var nextEmpId = emps.Any() ? emps.Max(x => x.Id) + 1 : 1;
            var emp = new Domain.Employee(nextEmpId, name, email, des, dept, Enum.TryParse<Domain.EmploymentType>(et, true, out var eType) ? eType : Domain.EmploymentType.FullTime, cr);
            await _empRepo.AddAsync(emp);
            Console.WriteLine("Employee added.");
        }
        else if (k == "d")
        {
            var id = ReadInt("Employee Id to deactivate: ");
            {
                var emp = await _empRepo.GetByIdAsync(id);
                if (emp is null) Console.WriteLine("Not found");
                else { emp.Deactivate(); await _empRepo.UpdateAsync(emp); Console.WriteLine("Deactivated"); }
            }
        }
    }

    private async Task ProjectMenu()
    {
        Console.WriteLine("Projects:");
        var projs = (await _projRepo.GetAllAsync()).ToList();
        foreach (var p in projs) Console.WriteLine($"{p.Id} {p.Name} {p.Status} {p.StartDate} - {p.EndDate}");
        Console.WriteLine("b) Back");
        Console.ReadLine();
    }

    private async Task AllocationMenu()
    {
        Console.WriteLine("Allocations:");
        var allocs = (await _allocRepo.GetAllAsync()).ToList();
        foreach (var a in allocs) Console.WriteLine($"{a.Id} Emp:{a.EmployeeId} Proj:{a.ProjectId} {a.StartDate}-{a.EndDate} {a.Percentage}% {a.Status}");
        Console.WriteLine("c) Create allocation, r) Release allocation, b) Back");
        var k = Console.ReadLine();
        if (k == "c")
        {
            var eid = ReadInt("EmployeeId: ");
            var pid = ReadInt("ProjectId: ");
            var sd = ReadDate("Start (yyyy-MM-dd): ");
            var ed = ReadDate("End (yyyy-MM-dd): ");
            var pct = ReadInt("Percentage: ", 1, 100);
            try
            {
                var a = await _allocSvc.CreateAsync(eid, pid, sd, ed, pct);
                Console.WriteLine($"Created allocation {a.Id}");
            }
            catch (Exception ex) { Console.WriteLine($"Failed: {ex.Message}"); }
        }
        else if (k == "r")
        {
            var aid = ReadInt("AllocationId: ");
            try { await _allocSvc.ReleaseAsync(aid); Console.WriteLine("Released"); }
            catch (Exception ex) { Console.WriteLine($"Failed: {ex.Message}"); }
        }
    }

    private async Task MatchingMenu()
    {
        var pid = ReadInt("ProjectId: ");
        var sd = ReadDate("Start (yyyy-MM-dd): ");
        Console.Write("End (yyyy-MM-dd): "); if (!DateOnly.TryParse(Console.ReadLine(), out var ed)) return;
        var results = (await _matcher.FindMatchesAsync(pid, sd, ed)).ToList();
        if (!results.Any()) Console.WriteLine("No matches");
        foreach (var r in results)
        {
            Console.WriteLine($"{r.EmployeeName} Score:{r.Score} Matched:{string.Join(',', r.MatchedSkills)} Missing:{string.Join(',', r.MissingSkills)}");
        }
    }

    private async Task BenchReport()
    {
        Console.WriteLine("Bench Report (threshold 50%)");
        var emps = (await _empRepo.GetAllAsync()).ToList();
        var allocs = (await _allocRepo.GetAllAsync()).Where(a => a.Status == Domain.AllocationStatus.Active).ToList();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        foreach (var e in emps)
        {
            var pct = allocs.Where(a => a.EmployeeId == e.Id && a.StartDate <= today && a.EndDate >= today).Sum(a => a.Percentage);
            Console.WriteLine($"{e.Name} Dept:{e.Department} Util:{pct}%");
        }
    }

    private async Task ProjectStaffing()
    {
        Console.WriteLine("Project Staffing Summary");
        var projs = (await _projRepo.GetAllAsync()).ToList();
        var allocs = (await _allocRepo.GetAllAsync()).Where(a => a.Status == Domain.AllocationStatus.Active).ToList();
        foreach (var p in projs)
        {
            var list = allocs.Where(a => a.ProjectId == p.Id)
                .Select(a => a.Percentage).Sum();
            Console.WriteLine($"{p.Name} {p.Status} {p.StartDate}-{p.EndDate} Allocated:{list}%");
        }
    }

    private async Task DepartmentUtilization()
    {
        Console.WriteLine("Department Utilization");
        var emps = (await _empRepo.GetAllAsync()).ToList();
        var allocs = (await _allocRepo.GetAllAsync()).Where(a => a.Status == Domain.AllocationStatus.Active).ToList();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var groups = emps.GroupBy(e => e.Department);
        foreach (var g in groups)
        {
            var totalEmployees = g.Count();
            var totalAlloc = g.Sum(e => allocs.Where(a => a.EmployeeId == e.Id && a.StartDate <= today && a.EndDate >= today).Sum(a => a.Percentage));
            var utilization = totalEmployees == 0 ? 0 : totalAlloc / (decimal)(totalEmployees * 100) * 100;
            Console.WriteLine($"Dept:{g.Key} Utilization:{utilization:0.##}% Employees:{totalEmployees}");
        }
    }
}
