using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class PedidoScopedService : IPedidoScopedService
    {
        private readonly IPedidoMessageService _pedidoMessageService;
        private readonly ILogger<PedidoScopedService> _logger;

        public PedidoScopedService(IPedidoMessageService pagamentoMessageService, ILogger<PedidoScopedService> logger) 
        {
            _pedidoMessageService = pagamentoMessageService;
            _logger = logger;
        }

        public async Task DoWork(CancellationToken cancellationToken)
        {
            await _pedidoMessageService.ReceberMensagem();
        }
    }
}
