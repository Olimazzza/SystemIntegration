using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;


namespace Opgave_1;

public class Replier
{
    public void Run()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare("request_queue", false, false, false, null);
            
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($" [x] Modtog besked: {message}");
                
                var response = "Hello, Client!";
                var responseBytes = Encoding.UTF8.GetBytes(response);
                
                channel.BasicPublish("", ea.BasicProperties.ReplyTo, null, responseBytes);
                channel.BasicAck(ea.DeliveryTag, false);
                
                channel.BasicPublish(exchange: "",
                    routingKey: ea.BasicProperties.ReplyTo,
                    basicProperties: null,
                    body: responseBytes);
                
                channel.BasicConsume(queue: "request_queue",
                    autoAck: false,
                    consumer: consumer);

                Console.ReadLine();
            };
        }
    }
}