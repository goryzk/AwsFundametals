using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Publisher;

public class RabbitMqPublisher
{
    private readonly IConnection _connection;
    private readonly SemaphoreSlim lockSemaphore = new SemaphoreSlim(1);
    public RabbitMqPublisher()
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        };

        _connection = factory.CreateConnection();
    }

    public async Task PublishAsync<T>(T message, string queueName)
    {
        await Task.Run(() =>
        {
            using var channel = _connection.CreateModel();
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = Guid.NewGuid().ToString();

            channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var messageBody = JsonSerializer.Serialize(message);

            var body = Encoding.UTF8.GetBytes(messageBody);

            channel.BasicPublish(exchange: string.Empty, routingKey: queueName, basicProperties: properties, body: body);
        });
    }

    public async Task PublishWithRetryAsync<T>(T message, string queueName)
    {
        var dlqQueueName = $"{queueName}_dlq";
        var retryCount = 0;
        var delay = TimeSpan.FromSeconds(1);
        var messagePublished = false;
        var pubishMessageRetryCount = 3;

        while (!messagePublished && retryCount < pubishMessageRetryCount)
        {
            try
            {
                await this.lockSemaphore.WaitAsync();
                await this.PublishAsync(message, queueName);
                messagePublished = true;
            }
            catch (Exception ex)
            { 

                delay = CalculateExponentialBackoffDelay(delay, retryCount);
                retryCount++;

                await Task.Delay(delay);
            }
            finally
            {
                this.lockSemaphore.Release();
            }

            if (!messagePublished)
            {
                await this.PublishToDeadLetteringAsync(message, queueName, dlqQueueName, dlqQueueName, dlqQueueName);
            }
        }
    }

    /// <summary>
    /// The PublishToDeadLetteringAsync method is used to publish a message to a dead lettering queue.
    /// </summary>
    /// <param name="message">message.</param>
    /// <param name="standardQueueName">standardQueueName.</param>
    /// <param name="dlqExchange">dlqExchange.</param>
    /// <param name="dlqQueueName">dlqQueueName.</param>
    /// <param name="dlqRoutingKey">dlqRoutingKey.</param>
    /// <typeparam name="T">T.</typeparam>
    /// <returns>Task result.</returns>
    public async Task PublishToDeadLetteringAsync<T>(T message, string standardQueueName, string dlqExchange, string dlqQueueName, string dlqRoutingKey)
    {
        try
        {
            await Task.Run(() =>
            {
                using var channel = _connection.CreateModel();

                channel.QueueDeclare(standardQueueName, durable: true, exclusive: false, autoDelete: false,
                    arguments: null);

                channel.ExchangeDeclare(dlqExchange, ExchangeType.Direct, durable: true, autoDelete: false);
                channel.QueueDeclare(dlqQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                channel.QueueBind(dlqQueueName, dlqExchange, dlqRoutingKey, null);

                var queueArgs = new Dictionary<string, object>
                {
                    { "x-dead-letter-exchange", dlqExchange },
                    { "x-dead-letter-routing-key", dlqRoutingKey },
                };

                channel.QueueDeclare(standardQueueName, durable: true, exclusive: false, autoDelete: false,
                    arguments: queueArgs);

                var messageBody = JsonSerializer.Serialize(message);

                var body = Encoding.UTF8.GetBytes(messageBody);

                channel.BasicPublish(dlqExchange, dlqRoutingKey, null, body);
            });
        }
        catch (Exception ex)
        {
        }
    }

    private static TimeSpan CalculateExponentialBackoffDelay(TimeSpan delay, int retryCount)
    {
        double backoffMultiplier = Math.Pow(2, retryCount);
        TimeSpan backoffDelay = TimeSpan.FromTicks(delay.Ticks * (long)backoffMultiplier);
        return backoffDelay;
    }
}