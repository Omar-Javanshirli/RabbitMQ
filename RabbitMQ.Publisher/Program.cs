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

            //Novbe yaradirig. RabbitMQ-de datalar Memoryde tutulur. Yani komputere restart verildiyi zaman datalar itecek
            //Eger bunun olmagin istemirikse bool durable adli ikinci parametri True etdiyimiz zaman Novbelerimiz fiziki olarag
            //save edilecek komputere restart versek bele datalar silinmiyecek.
            //bool Exclusive => bu parmetri True versem Burdaki Novbeye sadace yuxarida yaranmis channel ile baglana bilerik.
            //bool AutoDelete => 4-cu parmatir olarag qebul edir. Eger bu Novbeye son Subscriber baglantisini keserse Novbeni 
            //aftomatic sekilde silecek.
            channel.QueueDeclare("hello-queue", true, false, false);

            Enumerable.Range(1, 50).ToList().ForEach(x =>
            {
                string message = $"Message {x}";

                //Mesaji byte-lara ceviririk
                var messageBody = Encoding.UTF8.GetBytes(message);

                //messaji Novbeye gonderirik
                channel.BasicPublish(string.Empty, "hello-queue", null, messageBody);

                Console.WriteLine($"Sended message: {message}");
            });

           
            Console.ReadLine();
        }
    }
}
