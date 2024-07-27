using Application.Interfaces;
using Domain.Entities.Input;
using Domain.EntitiesDTO;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Transactions;

namespace Application.Services
{
    public class PedidoMessageService : IPedidoMessageService
    {
        private readonly IPedidoMessageQueue _pedidoMessageQueue;
        private readonly IPedidoMessageQueueError _pedidoMessageQueueError;
        private readonly IPedidoService _pedidoService;
        private readonly ILogger<PedidoMessageService> _logger;

        public PedidoMessageService(IPedidoMessageQueue pedidoMessageQueue, IPedidoMessageQueueError pedidoMessageQueueError, IPedidoService pagamentoService,  ILogger<PedidoMessageService> logger) 
        { 
            _pedidoMessageQueue = pedidoMessageQueue;
            _pedidoMessageQueueError = pedidoMessageQueueError;
            _pedidoService = pagamentoService;
            _logger = logger;

            _pedidoMessageQueue.MessageReceived += ReceberMensagemAsync;
            _pedidoMessageQueueError.MessageReceived += ReceberMensagemAsyncError;
        }

        public async Task ReceberMensagens()
        {
            await _pedidoMessageQueue.StartListening();
            await _pedidoMessageQueueError.StartListening();
        }

        private async Task ReceberMensagemAsync(string mensagem)
        {
            try
            {
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {                    
                    PagamentoInput pagamentoInput = JsonSerializer.Deserialize<PagamentoInput>(mensagem);

                    if (pagamentoInput.statusPagamento.Equals("Aprovado"))
                    {
                        await _pedidoService.UpdateStatusPedido(pagamentoInput.idPedido, "Em Preparação");
                    }

                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem recebida.");
                throw; // Rethrow exception para garantir que a mensagem é reinfileirada via PedidoMessageQueue
            }
        }

        private async Task ReceberMensagemAsyncError(string mensagem)
        {
            try
            {
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    PedidoDTO pedido = JsonSerializer.Deserialize<PedidoDTO>(mensagem);

                    await _pedidoService.UpdateStatusPedido(pedido.IdPedido, "Cancelado");

                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem recebida.");
                throw; // Rethrow exception para garantir que a mensagem é reinfileirada via PedidoMessageQueueError
            }
        }

        public void Dispose()
        {
            _pedidoMessageQueue.MessageReceived -= ReceberMensagemAsync;
            _pedidoMessageQueueError.MessageReceived -= ReceberMensagemAsyncError;
            GC.SuppressFinalize(this);
        }
    }
}
