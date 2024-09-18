using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using Newtonsoft.Json;

namespace SystemIntegration
{

    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Vælg en mulighed:");
            Console.WriteLine("1. Send ETA-besked (Air Traffic Control)");
            Console.WriteLine("2. Modtag ETA-besked (Airport Information Center)");
            var choice = Console.ReadLine();

            if (choice == "1")
            {
                RunSender();
                RunReceiver();
            }
            else if (choice == "2")
            {
                RunReceiver();
            }
            else
            {
                Console.WriteLine("Ugyldigt valg. Afslutter programmet.");
            }
        }

        private static void RunSender()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "eta_queue",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var message = new
                {
                    flightNumber = "BC123",
                    airline = "Bluff City Airlines",
                    origin = "JFK",
                    destination = "BCA",
                    aircraftType = "Boeing 737",
                    currentPosition = new
                    {
                        latitude = 39.7817,
                        longitude = -89.6501,
                        altitude = 35000
                    },
                    speed = 450,
                    estimatedTimeOfArrival = "2024-09-11T18:45:00Z",
                    status = "On Time"
                };

                var messageBody = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(messageBody);

                channel.BasicPublish(exchange: "",
                    routingKey: "eta_queue",
                    basicProperties: null,
                    body: body);

                Console.WriteLine(" [x] Sent ETA message to Airport Information Center");
            }
        }

        private static void RunReceiver()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "eta_queue",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var etaMessage = JsonConvert.DeserializeObject<dynamic>(message);

                    Console.WriteLine($" [x] Received ETA message for Flight {etaMessage.flightNumber}");
                    Console.WriteLine($"Estimated Time of Arrival: {etaMessage.estimatedTimeOfArrival}");
                    Console.WriteLine($"Status: {etaMessage.status}");
                };
                
                channel.BasicConsume(queue: "eta_queue",
                    autoAck: true,
                    consumer: consumer);

                Console.WriteLine(" [*] Waiting for ETA messages. To exit press CTRL+C");
                Console.ReadLine();
            }
        }
    }
}

