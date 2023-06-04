using Microsoft.AspNetCore.Hosting;
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
using System.Threading.Channels;
using System.Threading.Tasks;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;





namespace RabbitMQ.WatermarkApp.Web.BackgroundServices
{
    public class ImageWatermarkProcessBackgroundService : BackgroundService
    {
        private readonly RabbitMQClientService rabbitMQClientService;
        private readonly ILogger<ImageWatermarkProcessBackgroundService> logger;
        private readonly IWebHostEnvironment webHostEnvironment;
        private IModel channel;

        public ImageWatermarkProcessBackgroundService(RabbitMQClientService rabbitMQClientService, ILogger<ImageWatermarkProcessBackgroundService> logger, IWebHostEnvironment webHostEnvironment)
        {
            this.rabbitMQClientService = rabbitMQClientService;
            this.logger = logger;
            this.webHostEnvironment = webHostEnvironment;
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
            //try
            //{
               
            //}
            //catch (Exception ex)
            //{
            //    this.logger.LogError(ex.Message);
            //}


            var productImageCreatedEvent = JsonSerializer.Deserialize<ProductImageCreatedEvent>(Encoding.UTF8.GetString(@event.Body.ToArray()));

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images", productImageCreatedEvent.ImageName);

            var siteName = "wwww.mysite.com";

            using var img = Image.FromFile(path);

            using var graphic = Graphics.FromImage(img);

            var font = new Font(FontFamily.GenericMonospace, 40, FontStyle.Bold, GraphicsUnit.Pixel);

            var textSize = graphic.MeasureString(siteName, font);

            var color = Color.FromArgb(128, 255, 255, 255);
            var brush = new SolidBrush(color);

            var position = new Point(img.Width - ((int)textSize.Width + 30), img.Height - ((int)textSize.Height + 30));


            graphic.DrawString(siteName, font, brush, position);

            img.Save("wwwroot/Images/watermarks/" + productImageCreatedEvent.ImageName);


            img.Dispose();
            graphic.Dispose();

            this.channel.BasicAck(@event.DeliveryTag, false);

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
    }
}
