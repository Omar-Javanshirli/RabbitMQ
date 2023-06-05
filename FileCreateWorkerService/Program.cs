using FileCreateWorkerService.Models;
using FileCreateWorkerService.Servicies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using System;

namespace FileCreateWorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    IConfiguration Configuration = hostContext.Configuration;

                    services.AddDbContext<NLayerDBContext>(opt =>
                    {
                        opt.UseSqlServer(Configuration.GetConnectionString("SqlServer"));
                    });

                    services.AddSingleton(sp =>
                    {
                        return new ConnectionFactory()
                        {
                            Uri = new Uri(Configuration.GetConnectionString("RabbitMQ")),
                            DispatchConsumersAsync = true
                        };
                    });
                    services.AddSingleton<RabbitMQClientService>();
                    services.AddHostedService<Worker>();
                });
    }
}
