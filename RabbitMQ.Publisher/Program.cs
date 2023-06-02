using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;

namespace RabbitMQ.Publisher
{
    internal class Program
    {
        public enum LogNames
        {
            Critical = 1,
            Error = 2,
            Warning = 3,
            Info = 4
        }
        static void Main(string[] args)
        {
            //Connection RabbitMQ.
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqps://wztsmmlc:kY7U_HYs6gPOHDGr3HId7Bn9ZEWgaue_@toad.rmq.cloudamqp.com/wztsmmlc");

            using var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            //Exchange yaratmag.
            channel.ExchangeDeclare("logs-topic", ExchangeType.Topic, durable: true);


            Random rmd = new Random();
            Enumerable.Range(1, 50).ToList().ForEach(x =>
            {
                LogNames log1 = (LogNames)rmd.Next(1, 5);
                LogNames log2 = (LogNames)rmd.Next(1, 5);
                LogNames log3 = (LogNames)rmd.Next(1, 5);

                var rootKey = $"{log1}.{log2}.{log3}";
                string message = $"log-type :  {log1}-{log2}-{log3}";

                //Mesaji byte-lara ceviririk
                var messageBody = Encoding.UTF8.GetBytes(message);

                //messaji Novbeye gonderirik
                channel.BasicPublish("logs-topic", rootKey, null, messageBody);
                Console.WriteLine($"Sended log: {message}");
            });

            Console.ReadLine();
        }
    }
}
