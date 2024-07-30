namespace Domain.Interfaces
{
    public interface IPedidoMessageSender
    {
        void SendMessage(string queueName, string message);
    }
}
