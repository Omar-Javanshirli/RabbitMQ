using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;

namespace RabbitMQ.Publisher
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Connection RabbitMQ.
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqps://wztsmmlc:kY7U_HYs6gPOHDGr3HId7Bn9ZEWgaue_@toad.rmq.cloudamqp.com/wztsmmlc");

            using var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            //Exchange yaratmag.
            channel.ExchangeDeclare("logs-fanout", ExchangeType.Fanout, durable: true);

            Enumerable.Range(1, 50).ToList().ForEach(x =>
            {
                string message = $"log {x}";

                //Mesaji byte-lara ceviririk
                var messageBody = Encoding.UTF8.GetBytes(message);

                //messaji Novbeye gonderirik
                channel.BasicPublish("logs-fanout","", null, messageBody);

                Console.WriteLine($"Sended message: {message}");
            });

            Console.ReadLine();
        }
    }
}
