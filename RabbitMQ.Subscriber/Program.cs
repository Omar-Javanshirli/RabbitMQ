using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace RabbitMQ.Subscriber
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


            channel.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(channel);
            var queueName = channel.QueueDeclare().QueueName;

            Dictionary<string, object> headers = new();
            headers.Add("format", "pdf");
            headers.Add("shape", "a4");
            headers.Add("x-match", "all");

            channel.QueueBind(queueName,"header-exchange",string.Empty,headers);

            Console.WriteLine("Loglari dinliyorun..."); 

            channel.BasicConsume(queueName, false, consumer);

            consumer.Received += (object sender, BasicDeliverEventArgs e) =>
             {
                 var message = Encoding.UTF8.GetString(e.Body.ToArray());

                 Thread.Sleep(1500);
                 Console.WriteLine("Gelen mesaj: " + message);

                 channel.BasicAck(e.DeliveryTag, false);
             };
            Console.ReadLine();
        }
    }
}