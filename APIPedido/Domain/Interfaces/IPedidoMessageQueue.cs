namespace Domain.Interfaces
{
    public interface IPedidoMessageQueue
    {
        event Func<string, Task> MessageReceived;
        Task StartListening();
        void ReenqueueMessage(string message);
    }
}
