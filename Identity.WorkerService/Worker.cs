using Joseco.Outbox.EFCore.Procesor;
using Joseco.DDD.Core.Abstractions;

namespace Identity.WorkerService;

public class Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            try
            {

                using (var scope = scopeFactory.CreateScope())
                {
                    OutboxProcessor<DomainEvent> processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor<DomainEvent>>();
                    await processor.Process(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error: {message}", ex.Message);
            }



            await Task.Delay(5000, stoppingToken);
        }
    }
}
