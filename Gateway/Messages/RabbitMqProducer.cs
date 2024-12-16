using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Gateway.Messages
{
    public class RabbitMqProducer : IMessageProducer
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly ILogger<RabbitMqProducer> _logger;

        public RabbitMqProducer(ILogger<RabbitMqProducer> logger)
        {
            _logger = logger;

            // Konfigurer connection factory
            _connectionFactory = new ConnectionFactory
            {
                HostName = "localhost", // Skift til din RabbitMQ-server
                Port = 5672,
                UserName = "guest", // Skift hvis nødvendigt
                Password = "guest"  // Skift hvis nødvendigt
            };
        }
        public async Task SendMessage<T>(T message)
        {
            _logger.LogInformation("Starting to send message to RabbitMQ.");    // sæt til f.eks. .MinimumLevel.Information() i Program.cs for at få disse beskeder i logs

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync(new List<string> { "localhost" }); // Angiv værter (hosts) som liste
                _logger.LogInformation("Connection to RabbitMQ established.");

                await using var channel = await connection.CreateChannelAsync();
                _logger.LogInformation("RabbitMQ channel created.");

                await channel.QueueDeclareAsync(
                    queue: "add-book-queue",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                _logger.LogInformation("Queue 'add-book-queue' declared.");

                // Create persistent properties for the message
                var properties = new BasicProperties
                {
                    Persistent = true
                };

                // Serialize and send

                var bookJson = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(bookJson);

                await channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: "book.add.webshop",
                    mandatory: true,
                    basicProperties: properties,
                    body: body);
                _logger.LogInformation("Message successfully published to RabbitMQ: {Message}", bookJson);
            }
            catch (BrokerUnreachableException ex)
            {
                _logger.LogError(ex, "Failed to connect to RabbitMQ.");
                throw;
            }
            catch (OperationInterruptedException ex)
            {
                _logger.LogError(ex, "RabbitMQ operation interrupted.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending a message to RabbitMQ.");
                throw;
            }
        }
    }
}
