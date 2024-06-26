using Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class PedidoWorkerService : BackgroundService
    {
        private readonly ILogger<PedidoWorkerService> _logger;
        public IServiceProvider Services { get; }

        public PedidoWorkerService(IServiceProvider services, ILogger<PedidoWorkerService> logger) 
        {
            Services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await DoWork(stoppingToken);
        }

        private async Task DoWork(CancellationToken cancellationToken)
        {
            using (var scope = Services.CreateScope())
            {
                IPedidoScopedService scopedProcessingService = scope.ServiceProvider.GetRequiredService<IPedidoScopedService>();
                await scopedProcessingService.DoWork(cancellationToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }
    }
}
