using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using Newtonsoft.Json;

namespace Publisher_Subscriber
{

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Vælg en mulighed:");
            Console.WriteLine("1. Send ETA-besked (Airport Information Center)");
            Console.WriteLine("2. Modtag ETA-besked / Opret flyselskab (Flyselskab Subscriber)");
            var choice = Console.ReadLine();

            if (choice == "1")
            {
                RunPublisher();
            }
            else if (choice == "2")
            { 
                RunSubscriber();
            }
            else
            {
                Console.WriteLine("Forkert input");
            }
        }

        // Publisher (Airport Information Center)
        private static void RunPublisher()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                
                channel.ExchangeDeclare(exchange: "eta_fanout", type: "fanout");

                
                var message = new
                {
                    flightNumber = "BC123",
                    airline = "Bluff City Airlines",
                    origin = "JFK",
                    destination = "BCA",
                    aircraftType = "Boeing 737",
                    estimatedTimeOfArrival = "2024-09-11T18:45:00Z",
                    status = "On Time"
                };
                
                var messageBody = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(messageBody);

                channel.BasicPublish(exchange: "eta_fanout",
                    routingKey: "",
                    basicProperties: null,
                    body: body);

                Console.WriteLine(" [x] Sendte ETA besked til alle flyselskaber.");
            }
        }

        // Subscriber (Flyselskab)
        private static void RunSubscriber()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                string[] queueNames = { "eta_queue", "Mazzas_queue", "KLM_queue", "Bluff City Airlines_queue" };

                foreach (var queueName in queueNames)
                {
                    channel.QueueDeclare(queue: queueName,
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);
                    
                    channel.QueueBind(queue: queueName,
                        exchange: "eta_fanout",
                        routingKey: "");

                    Console.WriteLine($" [*] Venter på ETA-beskeder i {queueName}.");
                    
                    var result = channel.BasicGet(queueName, true);
                    while (result != null)
                    {
                        var body = result.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var etaMessage = JsonConvert.DeserializeObject<dynamic>(message);

                        Console.WriteLine($" [x] Modtog ETA-besked for Fly {etaMessage.flightNumber} fra {queueName}");
                        Console.WriteLine($"Ankomsttidspunkt: {etaMessage.estimatedTimeOfArrival}");
                        Console.WriteLine($"Status: {etaMessage.status}");

                        result = channel.BasicGet(queueName, true);
                    }

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var etaMessage = JsonConvert.DeserializeObject<dynamic>(message);

                        Console.WriteLine($" [x] Modtog ETA-besked for Fly {etaMessage.flightNumber} fra {queueName}");
                        Console.WriteLine($"Ankomsttidspunkt: {etaMessage.estimatedTimeOfArrival}");
                        Console.WriteLine($"Status: {etaMessage.status}");
                    };

                    // Consumeren begynder at lytte efter beskeder
                    channel.BasicConsume(queue: queueName,
                        autoAck: true,
                        consumer: consumer);
                }

                Console.ReadLine();
            }
        }
    }
}