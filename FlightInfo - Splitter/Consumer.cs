using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

class Consumer
{
    public void Run()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            // Opret køer til passager og bagage
            channel.QueueDeclare(queue: "passenger_queue",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            channel.QueueDeclare(queue: "luggage_queue",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            // Passenger consumer
            var passengerConsumer = new EventingBasicConsumer(channel);
            passengerConsumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Received passenger message: {0}", message);
            };
            channel.BasicConsume(queue: "passenger_queue",
                                 autoAck: true,
                                 consumer: passengerConsumer);

            // Luggage consumer
            var luggageConsumer = new EventingBasicConsumer(channel);
            luggageConsumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Received luggage message: {0}", message);
            };
            channel.BasicConsume(queue: "luggage_queue",
                                 autoAck: true,
                                 consumer: luggageConsumer);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}