namespace IBOPE.DoubleBlind.Application.Interfaces;

public interface ITaskExecutionService
{
    bool IsRunning { get; }
    string Status { get; }
    event Action? StatusChanged;

    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}
