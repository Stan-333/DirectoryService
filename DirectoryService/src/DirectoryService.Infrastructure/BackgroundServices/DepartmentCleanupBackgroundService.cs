using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.BackgroundServices;

public sealed class DepartmentCleanupBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<DepartmentCleanupBackgroundService> _logger;
    private readonly DepartmentCleanupOptions _options;

    public DepartmentCleanupBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<DepartmentCleanupOptions> options,
        ILogger<DepartmentCleanupBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Фоновая очистка подразделений отключена конфигурацией");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                TimeSpan delay = GetDelayUntilNextRunUtc();
                _logger.LogInformation(
                    "Следующая фоновая очистка подразделений запланирована через {Delay} в {RunAtUtc} UTC",
                    delay,
                    _options.RunAtUtc);

                await Task.Delay(delay, stoppingToken);

                await using var scope = _serviceScopeFactory.CreateAsyncScope();
                var cleanupService = scope.ServiceProvider.GetRequiredService<IDepartmentCleanupService>();
                int deletedCount = await cleanupService.CleanupExpiredDepartmentsAsync(stoppingToken);

                _logger.LogInformation(
                    "Очистка подразделений завершена. Удалено записей: {DeletedCount}",
                    deletedCount);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Ошибка фоновой очистки подразделений");
            }
        }
    }

    private TimeSpan GetDelayUntilNextRunUtc()
    {
        DateTime now = DateTime.UtcNow;
        TimeSpan runAtUtc = _options.RunAtUtc;

        if (runAtUtc < TimeSpan.Zero || runAtUtc >= TimeSpan.FromDays(1))
        {
            runAtUtc = TimeSpan.FromHours(2);
        }

        DateTime nextRunUtc = now.Date.Add(runAtUtc);
        if (nextRunUtc <= now)
        {
            nextRunUtc = nextRunUtc.AddDays(1);
        }

        return nextRunUtc - now;
    }
}