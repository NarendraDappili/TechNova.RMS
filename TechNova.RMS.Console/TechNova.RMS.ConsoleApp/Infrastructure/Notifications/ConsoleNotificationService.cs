using System;
using System.Threading.Tasks;
using TechNova.RMS.ConsoleApp.Application.Services;

namespace TechNova.RMS.ConsoleApp.Infrastructure.Notifications;

internal sealed class ConsoleNotificationService : INotificationService
{
    public Task NotifyAsync(string message)
    {
        Console.WriteLine($"[NOTIFY] {message}");
        return Task.CompletedTask;
    }
}
