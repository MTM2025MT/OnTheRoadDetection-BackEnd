using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PotholeApi.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PotholeApi.Services.DashboardsServices
{
    /// <summary>
    /// Keeps the pothole dashboard cache warm by recomputing it for every officer once on
    /// startup and then every five minutes, so requests to <see cref="IPotholeDashboardsService.GetPotholeDashboardsServices"/>
    /// can be served from the cache instead of doing the underlying DB aggregation.
    /// </summary>
    public class DashboardCacheRefreshWorker : BackgroundService
    {
        private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(5);

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DashboardCacheRefreshWorker> _logger;

        public DashboardCacheRefreshWorker(IServiceProvider serviceProvider, ILogger<DashboardCacheRefreshWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(RefreshInterval);

            do
            {
                await RefreshAllAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task RefreshAllAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var officerRepository = scope.ServiceProvider.GetRequiredService<IOfficerRepository>();
            var dashboardsService = scope.ServiceProvider.GetRequiredService<IPotholeDashboardsService>();

            var officers = await officerRepository.GetAllAsync();

            foreach (var officer in officers)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    await dashboardsService.RefreshDashboardCacheAsync(officer.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "DashboardCacheRefreshWorker: failed to refresh dashboard cache for officer {OfficerId}", officer.Id);
                }
            }
        }
    }
}
