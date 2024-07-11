namespace Data.Messaging
{
    using Domain.Interfaces;
    using RabbitMQ.Client;
    using System.Text;

    public class PedidoMessageSender : IPedidoMessageSender
    {                
        private readonly string _hostname = Environment.GetEnvironmentVariable("RABBIT_HOSTNAME");
        private readonly string _username = Environment.GetEnvironmentVariable("RABBIT_USERNAME");
        private readonly string _password = Environment.GetEnvironmentVariable("RABBIT_PASSWORD");

        public PedidoMessageSender()
        {            
        }

        public void SendMessage(string queueName, string message)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                                     routingKey: queueName,
                                     basicProperties: null,
                                     body: body);

                Console.WriteLine($" [x] Sent {message}");
            }
        }
    }

}
