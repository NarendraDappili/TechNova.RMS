using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TechNova.RMS.ConsoleApp.Application.Services;

public interface IReportService
{
    Task<IReadOnlyList<BenchEntry>> GetBenchAsync(DateOnly onDate, int thresholdPercentage);
    Task<IReadOnlyList<ProjectStaffingEntry>> GetProjectStaffingAsync();
    Task<IReadOnlyList<DepartmentUtilizationEntry>> GetDepartmentUtilizationAsync(DateOnly onDate);
}

public sealed record BenchEntry(int EmployeeId, string EmployeeName, string Department, int AllocationPercentage);
public sealed record ProjectStaffingEntry(int ProjectId, string ProjectName, string Status, int AllocatedPercentage, int EmployeeCount);
public sealed record DepartmentUtilizationEntry(string Department, int EmployeeCount, decimal UtilizationPercentage);
