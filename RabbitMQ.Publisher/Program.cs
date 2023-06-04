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
            channel.ExchangeDeclare("logs-direct", ExchangeType.Direct, durable: true);

            Enum.GetNames(typeof(LogNames)).ToList().ForEach(x =>
            {
                var rootKey = $"route-{x}";
                var queueName = $"direct-queue-{x}";

                //Novbenin yaradilmasi
                channel.QueueDeclare(queueName, true, false);

                //Novbenin Exchange bind edilmesi
                channel.QueueBind(queueName, "logs-direct", rootKey);
            });


            Enumerable.Range(1, 50).ToList().ForEach(x =>
            {
                LogNames log = (LogNames)new Random().Next(1, 5);

                string message = $"log-type :  {log}";

                //Mesaji byte-lara ceviririk
                var messageBody = Encoding.UTF8.GetBytes(message);

                var rootKey = $"route-{log}";

                //messaji Novbeye gonderirik
                channel.BasicPublish("logs-direct", rootKey, null, messageBody);

                Console.WriteLine($"Sended log: {message}");
            });

            Console.ReadLine();
        }
    }
}
