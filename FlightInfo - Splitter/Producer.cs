using System;
using RabbitMQ.Client;
using System.Text;

namespace FlightInfo___Splitter
{
    public class Producer
    {
        private IModel _channel;

        public Producer()
        {
            // Create connection to RabbitMQ server
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();

            // Declare queues for passenger and luggage
            _channel.QueueDeclare(queue: "passenger_queue",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _channel.QueueDeclare(queue: "luggage_queue",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        public void SendPassengerMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "",
                routingKey: "passenger_queue",
                basicProperties: null,
                body: body);
            Console.WriteLine(" [x] Sent passenger message");
        }

        public void SendLuggageMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "",
                routingKey: "luggage_queue",
                basicProperties: null,
                body: body);
            Console.WriteLine(" [x] Sent luggage message");
        }
    }
}