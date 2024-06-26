using Application.Interfaces;
using Domain.Entities.Input;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Application.Services
{
    public class PedidoMessageService : IPedidoMessageService
    {
        private readonly IPedidoMessageQueue _pedidoMessageQueue;
        private readonly IPedidoService _pedidoService;
        private readonly ILogger<PedidoMessageService> _logger;

        public PedidoMessageService(IPedidoMessageQueue pagamentoMessageQueue, IPedidoService pagamentoService,  ILogger<PedidoMessageService> logger) 
        { 
            _pedidoMessageQueue = pagamentoMessageQueue;
            _pedidoService = pagamentoService;
            _logger = logger;
        }

        public async Task PublicarMensagem(string message)
        {
            throw new NotImplementedException();
        }

        public async Task ReceberMensagem()
        {            
            string mensagem = await _pedidoMessageQueue.ReceberMensagem();
            PagamentoInput pagamentoInput = JsonSerializer.Deserialize<PagamentoInput>(mensagem);

            if (pagamentoInput.StatusPagamento.Equals("Aprovado")) {
                await _pedidoService.UpdateStatusPedido(pagamentoInput.IdPedido, "Em Preparação");
            }
        }

        public virtual void Dispose()
        {            
            GC.SuppressFinalize(this);
        }
    }
}
