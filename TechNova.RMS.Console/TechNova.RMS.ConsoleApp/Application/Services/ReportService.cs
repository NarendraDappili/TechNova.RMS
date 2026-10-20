using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechNova.RMS.ConsoleApp.Application.Repositories;
using TechNova.RMS.ConsoleApp.Domain;

namespace TechNova.RMS.ConsoleApp.Application.Services;

public sealed class ReportService(
    IEmployeeRepository employees,
    IProjectRepository projects,
    IAllocationRepository allocations) : IReportService
{
    public async Task<IReadOnlyList<BenchEntry>> GetBenchAsync(DateOnly onDate, int thresholdPercentage)
    {
        if (thresholdPercentage is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(thresholdPercentage));
        var employeeList = (await employees.GetAllAsync()).Where(employee => employee.IsActive).ToList();
        var allocationList = (await allocations.GetAllAsync()).Where(IsActiveOn(onDate)).ToList();
        return employeeList
            .Select(employee => new BenchEntry(employee.Id, employee.Name, employee.Department,
                allocationList.Where(allocation => allocation.EmployeeId == employee.Id).Sum(allocation => allocation.Percentage)))
            .Where(entry => entry.AllocationPercentage < thresholdPercentage)
            .OrderBy(entry => entry.AllocationPercentage).ThenBy(entry => entry.EmployeeName)
            .ToList();
    }

    public async Task<IReadOnlyList<ProjectStaffingEntry>> GetProjectStaffingAsync()
    {
        var projectList = (await projects.GetAllAsync()).ToList();
        var allocationList = (await allocations.GetAllAsync()).Where(allocation => allocation.Status == AllocationStatus.Active).ToList();
        return projectList.Select(project =>
        {
            var projectAllocations = allocationList.Where(allocation => allocation.ProjectId == project.Id).ToList();
            return new ProjectStaffingEntry(project.Id, project.Name, project.Status.ToString(),
                projectAllocations.Sum(allocation => allocation.Percentage),
                projectAllocations.Select(allocation => allocation.EmployeeId).Distinct().Count());
        }).OrderBy(entry => entry.ProjectName).ToList();
    }

    public async Task<IReadOnlyList<DepartmentUtilizationEntry>> GetDepartmentUtilizationAsync(DateOnly onDate)
    {
        var employeeList = (await employees.GetAllAsync()).Where(employee => employee.IsActive).ToList();
        var allocationList = (await allocations.GetAllAsync()).Where(IsActiveOn(onDate)).ToList();
        return employeeList.GroupBy(employee => employee.Department).Select(group =>
        {
            var allocated = group.Sum(employee => allocationList.Where(allocation => allocation.EmployeeId == employee.Id).Sum(allocation => allocation.Percentage));
            return new DepartmentUtilizationEntry(group.Key, group.Count(), group.Count() == 0 ? 0 : allocated / (decimal)group.Count());
        }).OrderBy(entry => entry.Department).ToList();
    }

    private static Func<Allocation, bool> IsActiveOn(DateOnly date) => allocation =>
        allocation.Status == AllocationStatus.Active && allocation.StartDate <= date && allocation.EndDate >= date;
}
