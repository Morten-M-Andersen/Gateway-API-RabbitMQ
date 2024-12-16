using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Gateway.Messages
{
    public class RabbitMqProducer : IMessageProducer
    {
        private readonly IConnectionFactory _connectionFactory;

        public RabbitMqProducer()
        {
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
            //var factory = new ConnectionFactory { HostName = "localhost" };
            //var connection = factory.CreateConnection();
            await using var connection = await _connectionFactory.CreateConnectionAsync(new List<string> { "localhost" }); // Angiv værter (hosts) som liste
            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: "add-book-queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

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
        }
    }
}
