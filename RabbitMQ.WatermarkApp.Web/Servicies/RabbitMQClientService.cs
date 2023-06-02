using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;

namespace RabbitMQ.WatermarkApp.Web.Servicies
{
    public class RabbitMQClientService : IDisposable
    {
        private readonly ConnectionFactory connectionFactory;
        private readonly ILogger<RabbitMQClientService> logger;
        private IConnection connection;
        private IModel channel;
        public static string ExchangeName = "ImageDirectExchange";
        public static string RoutingWatermark = "watermark-route-image";
        public static string QueueName = "queue-watermark-image";

        public RabbitMQClientService(ConnectionFactory connectionFactory, ILogger<RabbitMQClientService> logger)
        {
            this.connectionFactory = connectionFactory;
            this.logger = logger;
            Connect();
        }

        public IModel Connect()
        {
            this.connection = this.connectionFactory.CreateConnection();

            if (this.channel is { IsOpen: true })
                return this.channel;

            this.channel = this.connection.CreateModel();
            this.channel.ExchangeDeclare(ExchangeName, type: "direct", durable: true, autoDelete: false);
            this.channel.QueueDeclare(QueueName, durable: true, false, false, null);
            this.channel.QueueBind(queue: QueueName, exchange: ExchangeName, routingKey: RoutingWatermark);

            this.logger.LogInformation("RabbitMQ ile baglanti kuruldu...");

            return this.channel;
        }

        public void Dispose()
        {
            this.channel?.Close();
            this.channel?.Dispose();

            this.connection?.Close();
            this.connection?.Dispose();

            this.logger.LogInformation("RabbitMQ ile baglanti koptu...");
        }
    }
}
