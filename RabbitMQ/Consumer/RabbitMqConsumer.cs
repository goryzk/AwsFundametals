using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Consumer;

public class RabbitMqConsumer
{
    private readonly IConnection _connection;
    public RabbitMqConsumer()
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        };

        _connection = factory.CreateConnection();
    }

    public void ConsumeAndNotifyFinalStateTransactions(string queueName)
    {
        using var channel = _connection.CreateModel();

        channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            await Task.CompletedTask;
            if (model is not IModel localChannel)
            {
                return;
            }

            var body = ea.Body.ToArray();

            var message = Encoding.UTF8.GetString(body);

            var isNotified = message.Length % 2 == 0;
            if (isNotified)
            {
                Console.WriteLine($"Done:{message}");
                localChannel.BasicAck(ea.DeliveryTag, false);
            }
            else
            {
                Console.WriteLine($"Not Done:{message}");
            }
        };

        channel.BasicConsume(
            queue: queueName,
            autoAck: false,
            consumer: consumer);
    }
}