using ClosedXML.Excel;
using FileCreateWorkerService.Models;
using FileCreateWorkerService.Servicies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FileCreateWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;
        private readonly RabbitMQClientService rabbitMQClientService;

        //Databasa ya cata bilmey ucun IserviceProvider interfacinden istifade edirem
        private readonly IServiceProvider serviceProvider;
        private IModel channel;

        public Worker(ILogger<Worker> logger, RabbitMQClientService rabbitMQClientService, IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.rabbitMQClientService = rabbitMQClientService;
            this.serviceProvider = serviceProvider;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {

            this.channel = this.rabbitMQClientService.Connect();
            this.channel.BasicQos(0, 1, false);

            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var cunsomer = new AsyncEventingBasicConsumer(model: channel);

            this.channel.BasicConsume(RabbitMQClientService.QueueName, false, cunsomer);

            cunsomer.Received += Cunsomer_Received;
            return Task.CompletedTask;
        }

        private async Task Cunsomer_Received(object sender, BasicDeliverEventArgs @event)
        {
            await Task.Delay(5000);

            var createExcelMessage = JsonSerializer.Deserialize<CreateExcelMessage>(Encoding.UTF8.GetString(@event.Body.ToArray()));

            using var ms = new MemoryStream();

            //==============================================Excele cevrilme prosesi.=================================================
            var wb = new XLWorkbook();

            //bir databasa kimi fikielesin.
            var ds = new DataSet();

            //databasaya table elave edirik
            ds.Tables.Add(GetTable("categories"));
            wb.Worksheets.Add(ds);

            //databasani memoryde saxlayirig.
            wb.SaveAs(ms);

            MultipartFormDataContent multipartFormDataContent = new();
            multipartFormDataContent.Add(new ByteArrayContent(ms.ToArray()), "excelFile", Guid.NewGuid().ToString() + ".xlsx");

            var baseUrl = "https://localhost:5001/api/files";

            using (var httpClient = new HttpClient())
            {
                var respons = await httpClient.PostAsync($"{baseUrl}?fileId={createExcelMessage.FileId}", multipartFormDataContent);

                if (respons.IsSuccessStatusCode)
                {
                    this.logger.LogInformation($"File (Id : {createExcelMessage.FileId}) was created by successfully");
                    this.channel.BasicAck(@event.DeliveryTag, false);
                }
            }
        }

        private DataTable GetTable(string tableName)
        {
            List<Category> categories;

            using (var scope = this.serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<NLayerDBContext>();
                categories = context.Categories.ToList();
            }

            DataTable table = new() { TableName = tableName };
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("CreateDate", typeof(DateTime));
            table.Columns.Add("UpdateDate", typeof(DateTime));

            categories.ForEach(x =>
            {
                table.Rows.Add(x.Id, x.Name, x.CreateDate, x.UpdateDate);
            });

            return table;
        }
    }
}
