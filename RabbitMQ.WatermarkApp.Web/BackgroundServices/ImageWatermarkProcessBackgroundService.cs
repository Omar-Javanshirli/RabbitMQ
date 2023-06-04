using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.WatermarkApp.Web.Models;
using RabbitMQ.WatermarkApp.Web.Servicies;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.WatermarkApp.Web.BackgroundServices
{
    public class ImageWatermarkProcessBackgroundService : BackgroundService
    {
        private readonly RabbitMQClientService rabbitMQClientService;
        private readonly ILogger<ImageWatermarkProcessBackgroundService> logger;
        private IModel channel;

        public ImageWatermarkProcessBackgroundService(RabbitMQClientService rabbitMQClientService, ILogger<ImageWatermarkProcessBackgroundService> logger)
        {
            this.rabbitMQClientService = rabbitMQClientService;
            this.logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            this.channel = this.rabbitMQClientService.Connect();
            this.channel.BasicQos(0, 1, false);

            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var cunsumer = new AsyncEventingBasicConsumer(this.channel);

            this.channel.BasicConsume(queue: RabbitMQClientService.QueueName, false, cunsumer);

            cunsumer.Received += Cunsumer_Received;

            return Task.CompletedTask;
        }

        private Task Cunsumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            try
            {
                var imageCreateEvent = JsonSerializer.Deserialize<ProductImageCreatedEvent>
               (Encoding.UTF8.GetString(@event.Body.ToArray()));

                var websiteName = "www.myWeb.com";

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images", imageCreateEvent.ImageName);

                //Iamge.FromFile => path gonderirik ve elaqeli File Elde edirik.
                using var img = Image.FromFile(path);

                //Sekil uzerine yazi yaza bilemey ucun Graphics classina ehtiyacimiz olur.
                using var graphic = Graphics.FromImage(img);

                var font = new Font(FontFamily.GenericSerif, 42, FontStyle.Bold, GraphicsUnit.Pixel);
                var textSize = graphic.MeasureString(websiteName, font);
                var color = Color.FromArgb(128, 255, 255, 255);
                var brush = new SolidBrush(color);
                var position = new Point(img.Width - ((int)textSize.Width + 30), img.Height - ((int)textSize.Height + 30));

                graphic.DrawString(websiteName, font, brush, position);

                img.Save("wwwroot/Images/Watermark/" + imageCreateEvent.ImageName);

                img.Dispose();
                graphic.Dispose();

                this.channel.BasicAck(@event.DeliveryTag, false);
            }
            catch (System.Exception ex)
            {
                this.logger.LogError(ex.Message);
            }
           return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
    }
}
