using RabbitMQ.Client;
using System;
using System.Collections.Generic;
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
            channel.ExchangeDeclare("header-exchange", ExchangeType.Headers, durable: true);

            //headeri yaratmaq
            Dictionary<string, object> headers = new();
            headers.Add("format", "pdf");
            headers.Add("shape", "a4");

            //properti yaradirig. bu properti vasitesi ile headeri gonderirik.
            var properties=channel.CreateBasicProperties();
            properties.Headers = headers;

            //mesajin gonderilmesi.
            channel.BasicPublish("header-exchange", string.Empty, properties,Encoding.UTF8.GetBytes("header mesajlari"));

            Console.WriteLine("mesaj gonderilmisdir.");
            Console.ReadLine();
        }
    }
}
