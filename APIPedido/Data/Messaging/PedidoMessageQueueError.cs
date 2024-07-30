﻿using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Collections.Concurrent;
using System.Text;

namespace Data.Messaging
{
    public class PedidoMessageQueueError : IPedidoMessageQueueError, IDisposable
    {
        private readonly ILogger<PedidoMessageQueueError> _logger;
        private IConnection _connection;
        private IModel _channel;
        private bool _disposed = false;

        private readonly string _hostname = Environment.GetEnvironmentVariable("RABBIT_HOSTNAME");
        private readonly string _username = Environment.GetEnvironmentVariable("RABBIT_USERNAME");
        private readonly string _password = Environment.GetEnvironmentVariable("RABBIT_PASSWORD");
        private readonly Int16 _qtdeRetryPagamento = Convert.ToInt16(Environment.GetEnvironmentVariable("QTDE_RETRY_PAGAMENTO"));


        private ConcurrentDictionary<string, int> _retryCountDictionary = new ConcurrentDictionary<string, int>();

        public event Func<string, Task> MessageReceived;

        public PedidoMessageQueueError(ILogger<PedidoMessageQueueError> logger)
        {
            _logger = logger;
            ConnectRabbitMQ();
        }

        public async Task StartListening()
        {
            EnsureNotDisposed();

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (ch, ea) =>
            {
                string content = Encoding.UTF8.GetString(ea.Body.ToArray());

                try
                {
                    if (MessageReceived != null)
                    {
                        await MessageReceived(content);
                    }
                    // Ack se a mensagem foi processada com sucesso
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem, tentando recolocar na fila...");

                    if (_retryCountDictionary.TryGetValue(content, out int retryCount))
                    {
                        if (retryCount < 3) // Máximo de tentativas
                        {
                            _retryCountDictionary.AddOrUpdate(content, 1, (key, oldValue) => oldValue + 1);
                            // Rejeita e reenfileira a mensagem
                            _channel.BasicNack(ea.DeliveryTag, false, true);
                        }
                        else
                        {
                            _logger.LogError($"Falha ao processar a mensagem após {retryCount} tentativas, descartando a mensagem.");
                            _channel.BasicAck(ea.DeliveryTag, false); // Ack para descartar a mensagem
                            _retryCountDictionary.TryRemove(content, out _); // Remove do dicionário de retry
                        }
                    }
                    else
                    {
                        // Adiciona ao dicionário de retry
                        _retryCountDictionary.TryAdd(content, 1);
                        // Rejeita e reenfileira a mensagem
                        _channel.BasicNack(ea.DeliveryTag, false, true);
                    }
                }
            };

            _channel.BasicConsume("pagamento_erro", false, consumer);
        }

        public void ReenqueueMessage(string message)
        {
            EnsureNotDisposed();
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish("pagamento_erro_exchange", "pagamento_erro.*", null, body);
        }

        private void ConnectRabbitMQ()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password,
                Port = 5671, // Porta para SSL
                Ssl = new SslOption
                {
                    Enabled = true,
                    ServerName = _hostname, // Ou o nome do servidor conforme certificado
                    Version = System.Security.Authentication.SslProtocols.Tls12 // Certifique-se de que a versão TLS é suportada pelo seu servidor
                },
                DispatchConsumersAsync = true,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(60), // Timeout de conexão
                SocketReadTimeout = TimeSpan.FromSeconds(60), // Timeout de leitura
                SocketWriteTimeout = TimeSpan.FromSeconds(60) // Timeout de escrita
            };

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare("pagamento_erro_exchange", ExchangeType.Direct);
                _channel.QueueDeclare("pagamento_erro", false, false, false, null);
                _channel.QueueBind("pagamento_erro", "pagamento_erro_exchange", "pagamento_erro.*", null);
                _channel.BasicQos(0, 1, false);

                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

                _logger.LogInformation("Conectado ao RabbitMQ");
            }
            catch (BrokerUnreachableException ex)
            {
                _logger.LogError(ex, "Não foi possível alcançar o broker");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao tentar conectar ao RabbitMQ");
                throw;
            }
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogInformation("Conexão RabbitMQ encerrada.");
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(PedidoMessageQueueError));
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            _channel?.Close();
            _connection?.Close();
        }
    }
}
