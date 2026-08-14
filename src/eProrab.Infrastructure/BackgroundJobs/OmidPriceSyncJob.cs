using eProrab.Application.Features.MaterialPrices.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace eProrab.Infrastructure.BackgroundJobs
{
    public class OmidPriceSyncJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OmidPriceSyncJob> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(6);

        public OmidPriceSyncJob(IServiceScopeFactory scopeFactory, ILogger<OmidPriceSyncJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    var count = await mediator.Send(new SyncOmidPricesCommand(), stoppingToken);
                    _logger.LogInformation("Omid.az sync completed, {Count} prices updated", count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Omid.az sync job failed");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}