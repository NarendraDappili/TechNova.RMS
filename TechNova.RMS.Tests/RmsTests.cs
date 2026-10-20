using TechNova.RMS.ConsoleApp.Application.Repositories;
using TechNova.RMS.ConsoleApp.Application.Services;
using TechNova.RMS.ConsoleApp.Application.Services.Matching;
using TechNova.RMS.ConsoleApp.Domain;
using Xunit;

namespace TechNova.RMS.Tests;

public sealed class RmsTests
{
    private static readonly DateOnly Start = new(2026, 1, 1);
    private static readonly DateOnly End = new(2026, 1, 31);

    [Fact]
    public void Allocation_rejects_invalid_percentage_and_range()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Allocation(1, 1, 1, Start, End, 101));
        Assert.Throws<ArgumentException>(() => new Allocation(1, 1, 1, End, Start, 50));
    }

    [Fact]
    public async Task Inactive_employee_cannot_be_allocated()
    {
        var employee = new Employee(1, "Inactive", "inactive@example.com", "Developer", "Engineering", EmploymentType.FullTime, 100);
        employee.Deactivate();
        var service = CreateAllocationService([employee], [new Project(1, "Project", "Client", Start, End, ProjectStatus.Active)], []);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(1, 1, Start, End, 50));
    }

    [Fact]
    public async Task Overlapping_allocations_cannot_exceed_one_hundred_percent()
    {
        var employee = new Employee(1, "Active", "active@example.com", "Developer", "Engineering", EmploymentType.FullTime, 100);
        var project = new Project(1, "Project", "Client", Start, End, ProjectStatus.Active);
        var repository = new MemoryAllocationRepository();
        var service = CreateAllocationService([employee], [project], repository.Items, repository);

        await service.CreateAsync(1, 1, Start, End, 60);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(1, 1, Start.AddDays(1), End.AddDays(-1), 41));
    }

    [Fact]
    public async Task Allocation_must_stay_inside_project_dates_and_release_is_idempotent()
    {
        var employee = new Employee(1, "Active", "active@example.com", "Developer", "Engineering", EmploymentType.FullTime, 100);
        var project = new Project(1, "Project", "Client", Start, End, ProjectStatus.Active);
        var allocations = new MemoryAllocationRepository();
        var notifications = new MemoryNotificationService();
        var service = CreateAllocationService([employee], [project], allocations.Items, allocations, notifications);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(1, 1, Start.AddDays(-1), End, 50));
        var allocation = await service.CreateAsync(1, 1, Start, End, 50);
        await service.ReleaseAsync(allocation.Id);
        await service.ReleaseAsync(allocation.Id);

        Assert.Equal(1, notifications.Messages.Count(message => message.Contains("released", StringComparison.OrdinalIgnoreCase)));
        Assert.Equal(AllocationStatus.Released, (await allocations.GetByIdAsync(allocation.Id))!.Status);
    }

    [Fact]
    public async Task Matcher_returns_skill_explanation_and_excludes_fully_allocated_employee()
    {
        var match = new Employee(1, "Skilled", "skilled@example.com", "Developer", "Engineering", EmploymentType.FullTime, 100);
        match.AddOrUpdateSkill("C#", new Proficiency(5));
        var unavailable = new Employee(2, "Busy", "busy@example.com", "Developer", "Engineering", EmploymentType.FullTime, 100);
        var project = new Project(1, "Project", "Client", Start, End, ProjectStatus.Active);
        project.AddOrUpdateRequirement("C#", new Proficiency(4));
        var matcher = new SimpleMatcher(new MemoryEmployeeRepository([match, unavailable]), new MemoryProjectRepository([project]), new MemoryAllocationRepository([new Allocation(1, 2, 1, Start, End, 100)]));

        var results = (await matcher.FindMatchesAsync(1, Start, End)).ToList();

        var result = Assert.Single(results);
        Assert.Equal("Skilled", result.EmployeeName);
        Assert.Contains("C#", result.MatchedSkills);
    }

    [Fact]
    public async Task Reports_calculate_bench_and_department_utilization()
    {
        var alice = new Employee(1, "Alice", "alice@example.com", "Developer", "Engineering", EmploymentType.FullTime, 100);
        var bob = new Employee(2, "Bob", "bob@example.com", "Developer", "Engineering", EmploymentType.FullTime, 100);
        var project = new Project(1, "Project", "Client", Start, End, ProjectStatus.Active);
        var allocations = new MemoryAllocationRepository([new Allocation(1, 1, 1, Start, End, 50)]);
        var reports = new ReportService(new MemoryEmployeeRepository([alice, bob]), new MemoryProjectRepository([project]), allocations);

        var bench = await reports.GetBenchAsync(new(2026, 1, 10), 50);
        var utilization = await reports.GetDepartmentUtilizationAsync(new(2026, 1, 10));

        Assert.Equal("Bob", Assert.Single(bench).EmployeeName);
        Assert.Equal(25m, Assert.Single(utilization).UtilizationPercentage);
    }

    private static AllocationService CreateAllocationService(IEnumerable<Employee> employees, IEnumerable<Project> projects, IEnumerable<Allocation> allocations, IAllocationRepository? allocationRepository = null, INotificationService? notifications = null) =>
        new(allocationRepository ?? new MemoryAllocationRepository(allocations), new MemoryEmployeeRepository(employees), new MemoryProjectRepository(projects), notifications ?? new MemoryNotificationService());

    private sealed class MemoryEmployeeRepository(IEnumerable<Employee> items) : IEmployeeRepository
    {
        private readonly List<Employee> _items = items.ToList();
        public Task AddAsync(Employee e) { _items.Add(e); return Task.CompletedTask; }
        public Task<IEnumerable<Employee>> GetAllAsync() => Task.FromResult<IEnumerable<Employee>>(_items.ToList());
        public Task<Employee?> GetByIdAsync(int id) => Task.FromResult(_items.FirstOrDefault(item => item.Id == id));
        public Task UpdateAsync(Employee e) => Task.CompletedTask;
    }

    private sealed class MemoryProjectRepository(IEnumerable<Project> items) : IProjectRepository
    {
        private readonly List<Project> _items = items.ToList();
        public Task AddAsync(Project p) { _items.Add(p); return Task.CompletedTask; }
        public Task<IEnumerable<Project>> GetAllAsync() => Task.FromResult<IEnumerable<Project>>(_items.ToList());
        public Task<Project?> GetByIdAsync(int id) => Task.FromResult(_items.FirstOrDefault(item => item.Id == id));
        public Task UpdateAsync(Project p) => Task.CompletedTask;
    }

    private sealed class MemoryAllocationRepository : IAllocationRepository
    {
        public MemoryAllocationRepository(IEnumerable<Allocation>? items = null) => Items = items?.ToList() ?? [];
        public List<Allocation> Items { get; }
        public Task AddAsync(Allocation a) { Items.Add(a); return Task.CompletedTask; }
        public Task<IEnumerable<Allocation>> GetAllAsync() => Task.FromResult<IEnumerable<Allocation>>(Items.ToList());
        public Task<Allocation?> GetByIdAsync(int id) => Task.FromResult(Items.FirstOrDefault(item => item.Id == id));
        public Task UpdateAsync(Allocation a) => Task.CompletedTask;
    }

    private sealed class MemoryNotificationService : INotificationService
    {
        public List<string> Messages { get; } = [];
        public Task NotifyAsync(string message) { Messages.Add(message); return Task.CompletedTask; }
    }
}


