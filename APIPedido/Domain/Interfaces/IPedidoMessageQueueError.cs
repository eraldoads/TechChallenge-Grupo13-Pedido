namespace Domain.Interfaces
{
    public interface IPedidoMessageQueueError
    {
        event Func<string, Task> MessageReceived;
        Task StartListening();
        void ReenqueueMessage(string message);
    }
}
