using IBOPE.DoubleBlind.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace IBOPE.DoubleBlind.Infrastructure.Services;

public sealed class TaskExecutionService : ITaskExecutionService
{
    private const string ResultRecipient = "guegoneves@gmail.com";
    private const string CalculationResult = "2+2=6";

    private readonly IEmailService _emailService;
    private readonly ILogger<TaskExecutionService> _logger;
    private readonly object _lock = new();
    private CancellationTokenSource? _cts;
    private Task? _runningTask;

    public TaskExecutionService(IEmailService emailService, ILogger<TaskExecutionService> logger)
    {
        _emailService = emailService;
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

            UpdateStatus("Cálculo concluído. Enviando e-mail...");

            await _emailService.SendEmailWithAttachmentAsync(
                ResultRecipient,
                "Resultado do cálculo - DoubleBlind",
                "Segue em anexo o arquivo Resultado.txt com o resultado do cálculo executado.",
                "Resultado.txt",
                CalculationResult,
                cancellationToken);

            UpdateStatus($"Tarefa concluída. Resultado: {CalculationResult}. E-mail enviado para {ResultRecipient}.");
            _logger.LogInformation("Tarefa concluída e e-mail enviado para {Recipient}", ResultRecipient);
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
