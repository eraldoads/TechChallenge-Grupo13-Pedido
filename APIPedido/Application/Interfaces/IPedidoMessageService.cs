using System;
using System.Net;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPedidoMessageService : IDisposable
    {
        Task ReceberMensagem();
        Task PublicarMensagem(string message);
    }
}
