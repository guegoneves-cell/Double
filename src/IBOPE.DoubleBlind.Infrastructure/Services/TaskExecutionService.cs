using System.Text;
using IBOPE.DoubleBlind.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace IBOPE.DoubleBlind.Infrastructure.Services;

public sealed class TaskExecutionService : ITaskExecutionService
{
    private const string ResultFilePath = @"C:\Users\Public\ResultadoExemploTemplate.txt";

    private readonly ILogger<TaskExecutionService> _logger;
    private readonly object _lock = new();
    private CancellationTokenSource? _cts;
    private Task? _runningTask;

    public TaskExecutionService(ILogger<TaskExecutionService> logger)
    {
        _logger = logger;
    }

    public bool IsRunning { get; private set; }
    public string Status { get; private set; } = "Aguardando";

    public event Action? StatusChanged;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (IsRunning)
            {
                return Task.CompletedTask;
            }

            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            IsRunning = true;
            Status = "Iniciando tarefa...";
            _runningTask = RunTaskAsync(_cts.Token);
        }

        NotifyStatusChanged();
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        Task? task;
        CancellationTokenSource? cts;

        lock (_lock)
        {
            if (!IsRunning || _cts is null)
            {
                return;
            }

            cts = _cts;
            task = _runningTask;
        }

        await cts.CancelAsync();

        if (task is not null)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Esperado ao interromper a tarefa.
            }
        }
    }

    private async Task RunTaskAsync(CancellationToken cancellationToken)
    {
        try
        {
            UpdateStatus("Executando cálculo...");

            for (var step = 0; step < 10; step++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(500, cancellationToken);
            }

            UpdateStatus("Cálculo concluído. Publicando resultado em arquivo...");

            int primeiroOperando = 2;
            int segundoOperando = 2;
            int resultado = primeiroOperando + segundoOperando;
            var calculationResult = $"{primeiroOperando}+{segundoOperando}={resultado}";

            await File.WriteAllTextAsync(ResultFilePath, calculationResult, Encoding.UTF8, cancellationToken);

            UpdateStatus($"Tarefa concluída. Resultado: {calculationResult}. Arquivo publicado em {ResultFilePath}.");
            _logger.LogInformation("Tarefa concluída e resultado publicado em {FilePath}", ResultFilePath);
        }
        catch (OperationCanceledException)
        {
            UpdateStatus("Tarefa interrompida pelo usuário.");
            _logger.LogInformation("Tarefa interrompida pelo usuário.");
        }
        catch (Exception ex)
        {
            UpdateStatus($"Erro ao executar tarefa: {ex.Message}");
            _logger.LogError(ex, "Erro ao executar tarefa");
        }
        finally
        {
            lock (_lock)
            {
                IsRunning = false;
                _runningTask = null;
                _cts?.Dispose();
                _cts = null;
            }

            NotifyStatusChanged();
        }
    }

    private void UpdateStatus(string status)
    {
        Status = status;
        NotifyStatusChanged();
    }

    private void NotifyStatusChanged() => StatusChanged?.Invoke();
}
