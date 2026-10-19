using System.Threading.Tasks;

namespace TechNova.RMS.ConsoleApp.Application.Services;

public interface INotificationService
{
    Task NotifyAsync(string message);
}
