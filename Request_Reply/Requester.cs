using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using Newtonsoft.Json;

namespace RequestReplyPattern
{
    public class Requester
    {
        public void Run()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var replyQueueName = "ReplyQueue";
                channel.QueueDeclare(queue: replyQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                var consumer = new EventingBasicConsumer(channel);
                var correlationId = Guid.NewGuid().ToString();
                var props = channel.CreateBasicProperties();
                
                props.CorrelationId = correlationId;
                props.ReplyTo = replyQueueName;

                var requestMessage = new { flightNumber = "BC123" };
                var messageBody = JsonConvert.SerializeObject(requestMessage);
                var body = Encoding.UTF8.GetBytes(messageBody);

                channel.BasicPublish(exchange: "",
                    routingKey: "eta_request_queue",
                    basicProperties: props,
                    body: body);

                Console.WriteLine(" [x] Sendte ETA-forespørgsel og venter på svar...");

                consumer.Received += (model, ea) =>
                {
                    if (ea.BasicProperties.CorrelationId == correlationId)
                    {
                        var response = Encoding.UTF8.GetString(ea.Body.ToArray());
                        Console.WriteLine($" [.] Modtog svar: {response}");
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                };

                channel.BasicConsume(consumer: consumer,
                    queue: replyQueueName,
                    autoAck: false);

                Console.ReadLine();
            }
        }
    }
}