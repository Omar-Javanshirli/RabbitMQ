using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCreateWorkerService.Servicies
{
    public class RabbitMQClientService : IDisposable
    {
        private readonly ConnectionFactory connectionFactory;
        private readonly ILogger<RabbitMQClientService> logger;
        private IConnection connection;
        private IModel channel;
        public static string QueueName = "queue-excel-file";

        public RabbitMQClientService(ConnectionFactory connectionFactory, ILogger<RabbitMQClientService> logger)
        {
            this.connectionFactory = connectionFactory;
            this.logger = logger;
        }

        public IModel Connect()
        {
            this.connection = this.connectionFactory.CreateConnection();

            if (this.channel is { IsOpen: true })
                return this.channel;

            this.channel = this.connection.CreateModel();
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
