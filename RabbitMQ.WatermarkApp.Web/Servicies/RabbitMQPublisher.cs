using RabbitMQ.Client;
using RabbitMQ.WatermarkApp.Web.Models;
using System.Text;
using System.Text.Json;

namespace RabbitMQ.WatermarkApp.Web.Servicies
{
    public class RabbitMQPublisher
    {
        private readonly RabbitMQClientService rabbitMQClientService;

        public RabbitMQPublisher(RabbitMQClientService rabbitMQClientService)
        {
            this.rabbitMQClientService = rabbitMQClientService;
        }

        public void Publish(ProductImageCreatedEvent productImageCreatedEvent)
        {
            var channel = this.rabbitMQClientService.Connect();

            //RabbitMQ gonderilecek mesaji serialize edirik.
            var bodyString = JsonSerializer.Serialize(productImageCreatedEvent);

            var bodyByte = Encoding.UTF8.GetBytes(bodyString);

            //Mesajimiz Memory-de qalmasin RabbitMQ-da.Fiziksel olarax save edilsin deye asagidaki kodlar yazilmalidir.
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(exchange: RabbitMQClientService.ExchangeName,routingKey: RabbitMQClientService.RoutingWatermark,
                basicProperties: properties, body: bodyByte);
        }
    }
}
