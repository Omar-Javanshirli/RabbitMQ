using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
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

            //Novbe yaradirig. RabbitMQ-de datalar Memoryde tutulur. Yani komputere restart verildiyi zaman datalar itecek
            //Eger bunun olmagin istemirikse bool durable adli ikinci parametri True etdiyimiz zaman Novbelerimiz fiziki olarag
            //save edilecek komputere restart versek bele datalar silinmiyecek.
            //bool Exclusive => bu parmetri True versem Burdaki Novbeye sadace yuxarida yaranmis channel ile baglana bilerik.
            //bool AutoDelete => 4-cu parmatir olarag qebul edir. Eger bu Novbeye son Subscriber baglantisini keserse Novbeni 
            //aftomatic sekilde silecek.
            channel.QueueDeclare("hello-queue", true, false, false);

            //Her bir Subscribere nece dene mesaj gedeciyinin bolgusun bu method ile ediriy.
            //Qebul ediler ilk parametir gonderilen mesajin olcusunu bildirir "0" yazasaq bu o demekdir ki
            //istenilen olcude melumat qebul edirem.
            //Ikinci parametir Subscriber nece eded mesaj qebul edecek onu bildirir.
            //bool global => Ucuncu parmetir onu bildirirki eger biz Burada True Versek Deyekki her Subscribere 6 eded mesaj gedecey
            //ve global parametrinide True-ya set etmisem bu zaman Bizim nece Eded Subscriberim var ona uygun olaraq bolub eyni anda
            //gonderecey mesel ucun 2 eded Subscireber varsa eyni anda 3 mesaj birine 3 mesaj digerine gonderecek.
            //Yox eger Global parametresini False-sa set etsek Tek bir seferde 1-ci Subscribere 6 eded mesaj gonderecek
            //Sora yene mesajlar olsa tek seferde 2-ci Subscribere gonderecek ve bu bele Subscriberlerin sayina gore davam edecek.
            channel.BasicQos(0, 1, false);

            //Subscriber(consumer) yaradirig
            var consumer = new EventingBasicConsumer(channel);

            //bool AutoAck => adli ikinci paremtire Eger ki True versek  RabbitMQ Subscribere bir mesaj gonderiyi zaman
            //bu mesaj dogruda olsa yanlisda olsa Novbeden silinecek. Eger ki bunu false versey biz demis oluruq ki 
            //sen bunu novbeden silme eger ki mesaj dogru olsa men seni xeberdar edecem Novbeden silmeyin ucun.
            channel.BasicConsume("hello-queue", false, consumer);

            //Event uzerinnen Qulax asma prosesini yazmaq
            consumer.Received += (object sender, BasicDeliverEventArgs e) =>
            {
                var message = Encoding.UTF8.GetString(e.Body.ToArray());
                Console.WriteLine("Gelen mesaj: " + message);

                Thread.Sleep(1500);

                //Bu method eger ki Novbeden geden mesaji hell ede bilibse bu zaman RabbitMQ-nu xebardar edirki
                //artiq sen elaqeli mesaji sile bilersen ve silir.
                //Sildikden sora burada ikinci parametre olaraq Multiple deyeri var. Eger buna true desek
                //o an hell edilmis ama RabbitMQ-ya catmamis basqa mesajdar varsa onun melumatlarini
                //RabbitMQ-ya xebardar eder.
                channel.BasicAck(e.DeliveryTag,false);
            };

            Console.ReadLine();
        }
    }
}