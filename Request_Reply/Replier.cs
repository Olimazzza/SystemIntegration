using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace RequestReplyPattern
{
    public class Replier
    {
        public void Run()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "eta_request_queue",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var flights = new Dictionary<string, (string estimatedTimeOfArrival, string status)>
                {
                    { "BC123", ("2024-09-11T18:45:00Z", "On Time") },
                    { "KL456", ("2024-09-11T19:00:00Z", "Delayed") }
                };

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var requestMessage = JsonConvert.DeserializeObject<dynamic>(message);
                    var flightNumber = (string)requestMessage.flightNumber;

                    var responseMessage = flights.ContainsKey(flightNumber)
                        ? new { flightNumber, flights[flightNumber].estimatedTimeOfArrival, flights[flightNumber].status }
                        : new { flightNumber, estimatedTimeOfArrival = "Unknown", status = "Unknown" };

                    var responseProps = channel.CreateBasicProperties();
                    responseProps.CorrelationId = ea.BasicProperties.CorrelationId;

                    var responseBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(responseMessage));

                    channel.BasicPublish(exchange: "",
                        routingKey: ea.BasicProperties.ReplyTo,
                        basicProperties: responseProps,
                        body: responseBytes);

                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                };

                channel.BasicConsume(queue: "eta_request_queue",
                    autoAck: false,
                    consumer: consumer);

                Console.ReadLine();
            }
        }
    }
}