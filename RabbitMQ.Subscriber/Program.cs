using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

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

            //Novbe yaradirig. RabbitMQ-de datalar Memoryde tutulur. Yani komputere restart verildiyi zaman datalar itecek
            //Eger bunun olmagin istemirikse bool durable adli ikinci parametri True etdiyimiz zaman Novbelerimiz fiziki olarag
            //save edilecek komputere restart versek bele datalar silinmiyecek.
            //bool Exclusive => bu parmetri True versem Burdaki Novbeye sadace yuxarida yaranmis channel ile baglana bilerik.
            //bool AutoDelete => 4-cu parmatir olarag qebul edir. Eger bu Novbeye son Subscriber baglantisini keserse Novbeni 
            //aftomatic sekilde silecek.
            channel.QueueDeclare("hello-queue", true, false, false);

            //Subscriber(consumer) yaradirig
            var consumer = new EventingBasicConsumer(channel);

            //bool AutoAck => adli ikinci paremtire Eger ki True versek  RabbitMQ Subscribere bir mesaj gonderiyi zaman
            //bu mesaj dogruda olsa yanlisda olsa Novbeden silinecek. Eger ki bunu false versey biz demis oluruq ki 
            //sen bunu novbeden silme eger ki mesaj dogru olsa men seni xeberdar edecem Novbeden silmeyin ucun.
            channel.BasicConsume("hello-queue", true, consumer);

            //Event uzerinnen Qulax asma prosesini yazmaq
            consumer.Received += (object sender, BasicDeliverEventArgs e) =>
            {
                var message = Encoding.UTF8.GetString(e.Body.ToArray());
                Console.WriteLine("Gelen mesaj: " + message);
            };

            Console.ReadLine();
        }
    }
}