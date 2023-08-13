using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "TransactionIdPublisher-Queue_error",
                     durable: true,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

Console.WriteLine(" [*] Waiting for messages.");

var consumer = new EventingBasicConsumer(channel);
consumer.Received += async (model, ea) =>
{
    try
	{
        await Task.CompletedTask;
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        Console.WriteLine($"{message} Removed from queue");
        channel.BasicAck(ea.DeliveryTag, false);
    }
	catch (Exception ex)
	{

        Console.WriteLine($"Exception:{ex}");
	}
};

channel.BasicConsume(queue: "TransactionIdPublisher-Queue_error",
                     autoAck: false,
                     consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();