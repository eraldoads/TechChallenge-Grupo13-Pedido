namespace Domain.Interfaces
{
    public interface IPedidoMessageQueue
    {
        Task<string> ReceberMensagem();
    }
}
