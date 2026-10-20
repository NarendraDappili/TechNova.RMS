using System;
using System.Linq;
using System.Threading.Tasks;
using TechNova.RMS.ConsoleApp.Application.Repositories;
using TechNova.RMS.ConsoleApp.Application.Services;
using TechNova.RMS.ConsoleApp.Application.Services.Matching;
using TechNova.RMS.ConsoleApp.Domain;

namespace TechNova.RMS.ConsoleApp;

public sealed class ConsoleMenu(
    IEmployeeRepository employees,
    IProjectRepository projects,
    IAllocationRepository allocations,
    IEmployeeService employeeService,
    IProjectService projectService,
    IAllocationService allocationService,
    IReportService reportService,
    IMatcher matcher)
{
    public async Task RunAsync()
    {
        await SeedData.EnsureSeedAsync(employees, projects, allocations);
        while (true)
        {
            Console.WriteLine("\nTechNova RMS - Main Menu");
            Console.WriteLine("1. Employee management");
            Console.WriteLine("2. Project management");
            Console.WriteLine("3. Allocation and release");
            Console.WriteLine("4. Employee matching");
            Console.WriteLine("5. Bench report");
            Console.WriteLine("6. Project staffing summary");
            Console.WriteLine("7. Department utilization");
            Console.WriteLine("9. Exit");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": await EmployeeMenuAsync(); break;
                case "2": await ProjectMenuAsync(); break;
                case "3": await AllocationMenuAsync(); break;
                case "4": await MatchingMenuAsync(); break;
                case "5": await BenchReportAsync(); break;
                case "6": await ProjectStaffingAsync(); break;
                case "7": await DepartmentUtilizationAsync(); break;
                case "9": return;
                default: Console.WriteLine("Invalid choice."); break;
            }
        }
    }

    private async Task EmployeeMenuAsync()
    {
        foreach (var employee in (await employees.GetAllAsync()).OrderBy(employee => employee.Id))
            Console.WriteLine($"{employee.Id}: {employee.Name} | {employee.Department} | {(employee.IsActive ? "Active" : "Inactive")} | Skills: {string.Join(", ", employee.Skills.Select(skill => $"{skill.Name}({skill.Proficiency.Value})"))}");
        Console.WriteLine("a) Add  u) Update  d) Deactivate  s) Add/update skill  r) Remove skill  b) Back");
        try
        {
            switch (Console.ReadLine()?.Trim().ToLowerInvariant())
            {
                case "a":
                    var added = await employeeService.AddAsync(ReadRequired("Name: "), ReadRequired("Email: "), ReadRequired("Designation: "), ReadRequired("Department: "), ReadEnum<EmploymentType>("Employment type: "), ReadDecimal("Cost rate: ", 0.01m));
                    Console.WriteLine($"Employee {added.Id} added.");
                    break;
                case "u":
                    var updateId = ReadInt("Employee ID: ");
                    await employeeService.UpdateAsync(updateId, ReadOptional("Name (blank keeps current): "), ReadOptional("Email (blank keeps current): "), ReadOptional("Designation (blank keeps current): "), ReadOptional("Department (blank keeps current): "), null, null);
                    Console.WriteLine("Employee updated.");
                    break;
                case "d":
                    await employeeService.DeactivateAsync(ReadInt("Employee ID: "));
                    Console.WriteLine("Employee deactivated.");
                    break;
                case "s":
                    await employeeService.AddOrUpdateSkillAsync(ReadInt("Employee ID: "), ReadRequired("Skill: "), ReadInt("Proficiency (1-5): ", 1, 5));
                    Console.WriteLine("Skill saved.");
                    break;
                case "r":
                    await employeeService.RemoveSkillAsync(ReadInt("Employee ID: "), ReadRequired("Skill: "));
                    Console.WriteLine("Skill removed.");
                    break;
            }
        }
        catch (Exception ex) { Console.WriteLine($"Operation failed: {ex.Message}"); }
    }

    private async Task ProjectMenuAsync()
    {
        foreach (var project in (await projects.GetAllAsync()).OrderBy(project => project.Id))
            Console.WriteLine($"{project.Id}: {project.Name} | {project.Client} | {project.Status} | {project.StartDate:yyyy-MM-dd} to {project.EndDate:yyyy-MM-dd} | Required: {string.Join(", ", project.Requirements.Select(requirement => $"{requirement.SkillName}({requirement.MinProficiency.Value})"))}");
        Console.WriteLine("a) Add  u) Update  s) Add/update required skill  r) Remove required skill  b) Back");
        try
        {
            switch (Console.ReadLine()?.Trim().ToLowerInvariant())
            {
                case "a":
                    var project = await projectService.AddAsync(ReadRequired("Name: "), ReadRequired("Client: "), ReadDate("Start (yyyy-MM-dd): "), ReadDate("End (yyyy-MM-dd): "), ReadEnum<ProjectStatus>("Status: "));
                    Console.WriteLine($"Project {project.Id} added.");
                    break;
                case "u":
                    await projectService.UpdateAsync(ReadInt("Project ID: "), ReadOptional("Name (blank keeps current): "), ReadOptional("Client (blank keeps current): "), null, null, null);
                    Console.WriteLine("Project updated.");
                    break;
                case "s":
                    await projectService.AddOrUpdateRequirementAsync(ReadInt("Project ID: "), ReadRequired("Skill: "), ReadInt("Minimum proficiency (1-5): ", 1, 5));
                    Console.WriteLine("Project requirement saved.");
                    break;
                case "r":
                    await projectService.RemoveRequirementAsync(ReadInt("Project ID: "), ReadRequired("Skill: "));
                    Console.WriteLine("Project requirement removed.");
                    break;
            }
        }
        catch (Exception ex) { Console.WriteLine($"Operation failed: {ex.Message}"); }
    }

    private async Task AllocationMenuAsync()
    {
        foreach (var allocation in (await allocations.GetAllAsync()).OrderBy(allocation => allocation.Id))
            Console.WriteLine($"{allocation.Id}: Employee {allocation.EmployeeId} -> Project {allocation.ProjectId} | {allocation.StartDate:yyyy-MM-dd} to {allocation.EndDate:yyyy-MM-dd} | {allocation.Percentage}% | {allocation.Status}");
        Console.WriteLine("c) Create  r) Release  b) Back");
        try
        {
            switch (Console.ReadLine()?.Trim().ToLowerInvariant())
            {
                case "c":
                    var allocation = await allocationService.CreateAsync(ReadInt("Employee ID: "), ReadInt("Project ID: "), ReadDate("Start (yyyy-MM-dd): "), ReadDate("End (yyyy-MM-dd): "), ReadInt("Percentage (1-100): ", 1, 100));
                    Console.WriteLine($"Allocation {allocation.Id} created.");
                    break;
                case "r":
                    await allocationService.ReleaseAsync(ReadInt("Allocation ID: "));
                    Console.WriteLine("Allocation released and notification sent.");
                    break;
            }
        }
        catch (Exception ex) { Console.WriteLine($"Operation failed: {ex.Message}"); }
    }

    private async Task MatchingMenuAsync()
    {
        try
        {
            var results = await matcher.FindMatchesAsync(ReadInt("Project ID: "), ReadDate("Start (yyyy-MM-dd): "), ReadDate("End (yyyy-MM-dd): "));
            foreach (var result in results)
                Console.WriteLine($"{result.EmployeeName} | Score: {result.Score} | Matched: {string.Join(", ", result.MatchedSkills)} | Missing: {string.Join(", ", result.MissingSkills)}");
            if (!results.Any()) Console.WriteLine("No available matches.");
        }
        catch (Exception ex) { Console.WriteLine($"Matching failed: {ex.Message}"); }
    }

    private async Task BenchReportAsync()
    {
        var report = await reportService.GetBenchAsync(DateOnly.FromDateTime(DateTime.UtcNow), 50);
        Console.WriteLine("Bench report (active employees below 50% allocation today):");
        foreach (var entry in report) Console.WriteLine($"{entry.EmployeeName} | {entry.Department} | {entry.AllocationPercentage}%");
    }

    private async Task ProjectStaffingAsync()
    {
        Console.WriteLine("Project staffing report:");
        foreach (var entry in await reportService.GetProjectStaffingAsync()) Console.WriteLine($"{entry.ProjectName} | {entry.Status} | {entry.EmployeeCount} employees | {entry.AllocatedPercentage}% total allocation");
    }

    private async Task DepartmentUtilizationAsync()
    {
        Console.WriteLine("Department utilization today:");
        foreach (var entry in await reportService.GetDepartmentUtilizationAsync(DateOnly.FromDateTime(DateTime.UtcNow))) Console.WriteLine($"{entry.Department} | {entry.EmployeeCount} active employees | {entry.UtilizationPercentage:0.##}%");
    }

    private static string ReadRequired(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var value = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(value)) return value;
            Console.WriteLine("This field is required.");
        }
    }

    private static string? ReadOptional(string prompt)
    {
        Console.Write(prompt);
        var value = Console.ReadLine()?.Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static int ReadInt(string prompt, int min = int.MinValue, int max = int.MaxValue)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out var value) && value >= min && value <= max) return value;
            Console.WriteLine($"Enter a whole number between {min} and {max}.");
        }
    }

    private static decimal ReadDecimal(string prompt, decimal min)
    {
        while (true)
        {
            Console.Write(prompt);
            if (decimal.TryParse(Console.ReadLine(), out var value) && value >= min) return value;
            Console.WriteLine("Enter a valid positive amount.");
        }
    }

    private static DateOnly ReadDate(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (DateOnly.TryParse(Console.ReadLine(), out var value)) return value;
            Console.WriteLine("Use date format yyyy-MM-dd.");
        }
    }

    private static T ReadEnum<T>(string prompt) where T : struct, Enum
    {
        while (true)
        {
            Console.Write(prompt);
            if (Enum.TryParse<T>(Console.ReadLine(), true, out var value)) return value;
            Console.WriteLine($"Enter one of: {string.Join(", ", Enum.GetNames<T>())}.");
        }
    }
}
