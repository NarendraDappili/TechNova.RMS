using System.Threading.Tasks;
using TechNova.RMS.ConsoleApp.Domain;

namespace TechNova.RMS.ConsoleApp.Application.Services;

public interface IProjectService
{
    Task<Project> AddAsync(string name, string client, DateOnly startDate, DateOnly endDate, ProjectStatus status);
    Task UpdateAsync(int id, string? name, string? client, DateOnly? startDate, DateOnly? endDate, ProjectStatus? status);
    Task AddOrUpdateRequirementAsync(int projectId, string skillName, int minProficiency);
    Task RemoveRequirementAsync(int projectId, string skillName);
}
