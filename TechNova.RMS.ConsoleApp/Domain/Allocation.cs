using System;

namespace TechNova.RMS.ConsoleApp.Domain;

public enum AllocationStatus { Active, Released }

public sealed class Allocation
{
    public Guid Id { get; }
    public Guid EmployeeId { get; }
    public Guid ProjectId { get; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public int Percentage { get; private set; }
    public AllocationStatus Status { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime? ReleasedAt { get; private set; }

    public Allocation(Guid id, Guid employeeId, Guid projectId, DateOnly start, DateOnly end, int percentage)
    {
        Id = id != Guid.Empty ? id : throw new ArgumentException("Id required", nameof(id));
        EmployeeId = employeeId != Guid.Empty ? employeeId : throw new ArgumentException("EmployeeId required", nameof(employeeId));
        ProjectId = projectId != Guid.Empty ? projectId : throw new ArgumentException("ProjectId required", nameof(projectId));
        if (end < start) throw new ArgumentException("End must be on or after start");
        if (percentage <= 0 || percentage > 100) throw new ArgumentOutOfRangeException(nameof(percentage));
        StartDate = start;
        EndDate = end;
        Percentage = percentage;
        Status = AllocationStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateDates(DateOnly start, DateOnly end)
    {
        if (end < start) throw new ArgumentException("End must be on or after start");
        StartDate = start;
        EndDate = end;
    }

    public void UpdatePercentage(int percentage)
    {
        if (percentage <= 0 || percentage > 100) throw new ArgumentOutOfRangeException(nameof(percentage));
        Percentage = percentage;
    }

    public void Release()
    {
        if (Status == AllocationStatus.Released) return;
        Status = AllocationStatus.Released;
        ReleasedAt = DateTime.UtcNow;
    }
}
