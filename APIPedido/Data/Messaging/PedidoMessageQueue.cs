using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Data.Messaging
{
    public class PedidoMessageQueue : IPedidoMessageQueue
    {
        private readonly ILogger _logger;
        private IConnection _connection;
        private IModel _channel;

        private readonly string _hostname = Environment.GetEnvironmentVariable("RABBIT_HOSTNAME");
        private readonly string _username = Environment.GetEnvironmentVariable("RABBIT_USERNAME");
        private readonly string _password = Environment.GetEnvironmentVariable("RABBIT_PASSWORD");

        public PedidoMessageQueue(ILogger<PedidoMessageQueue> logger) 
        {
            _logger = logger;
            ConnectRabbitMQ();
        }

        public Task<string> ReceberMensagem()
        {
            var tcs = new TaskCompletionSource<string>();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                string content = Encoding.UTF8.GetString(ea.Body.ToArray());
                HandleMessage(content);
                _channel.BasicAck(ea.DeliveryTag, false);

                // Sinaliza que a mensagem foi recebida e processada
                tcs.SetResult(content);
            };

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume("novo_pedido", false, consumer);

            // Retorna a Task que será completada quando a mensagem for recebida
            return tcs.Task;
        }


        private void ConnectRabbitMQ()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password
            };

            _connection = factory.CreateConnection();            
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare("novo_pedido_exchange", ExchangeType.Direct);
            _channel.QueueDeclare("novo_pedido", false, false, false, null);
            _channel.QueueBind("novo_pedido", "novo_pedido_exchange", "novo_pedido.*", null);
            _channel.BasicQos(0, 1, false);

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }

        private void HandleMessage(string content)
        {            
            _logger.LogInformation($"Mensagem recebida {content}");
        }

        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e) { }
        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerRegistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerShutdown(object sender, ShutdownEventArgs e) { }
        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) { }

        public void Dispose()
        {
            _channel.Close();
            _connection.Close();            
        }
    }
}
