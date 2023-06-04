using RabbitMQ.Client;
using Shared;
using System.Text;
using System.Text.Json;

namespace RabbitMQ.Web.ExcelCreate.Servicies
{
    public class RabbitMQPublisher
    {
        private readonly RabbitMQClientService rabbitMQClientService;

        public RabbitMQPublisher(RabbitMQClientService rabbitMQClientService)
        {
            this.rabbitMQClientService = rabbitMQClientService;
        }

        public void Publish( CreateExcelMessage createExcelMessage)
        {
            var channel = this.rabbitMQClientService.Connect();

            //RabbitMQ gonderilecek mesaji serialize edirik.
            var bodyString = JsonSerializer.Serialize(createExcelMessage);

            var bodyByte = Encoding.UTF8.GetBytes(bodyString);

            //Mesajimiz Memory-de qalmasin RabbitMQ-da.Fiziksel olarax save edilsin deye asagidaki kodlar yazilmalidir.
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(exchange: RabbitMQClientService.ExchangeName, routingKey: RabbitMQClientService.RoutingExcel,
                basicProperties: properties, body: bodyByte);
        }
    }
}
