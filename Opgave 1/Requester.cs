using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace Opgave_1;

public class Requester
{
    public void Run()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            var QueueName = channel.QueueDeclare("ReplyQueue", false, false, false, null);
            var consumer = new EventingBasicConsumer(channel);
            var correlationId = Guid.NewGuid().ToString();
            var props = channel.CreateBasicProperties();
            props.CorrelationId = correlationId;
            props.ReplyTo = QueueName;
            props.Expiration = "15000";
            
            var message = "Denne besked udløber om 15 sekunder!";
            var body = Encoding.UTF8.GetBytes(message);
            
            channel.BasicPublish(exchange: "",
                routingKey: "request_queue",
                basicProperties: props,
                body: body);
            
            Console.WriteLine(" [x] Sendte besked og venter på svar...");

            Console.ReadLine();
        }
    }
}