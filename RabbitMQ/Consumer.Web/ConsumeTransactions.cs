using Microsoft.AspNetCore.Connections;
using RabbitMQ.Client.Events;

namespace Consumer.Web
{
    public class ConsumeTransactions : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }

        private readonly IConnection _connection;
        public ConsumeTransactions()
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

            var dlqName = queueName + "_dlq";
            channel.QueueDeclare(queue: dlqName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            channel.QueueBind(queue: dlqName, exchange: string.Empty, routingKey: dlqName, new Dictionary<string, object>
        {
            { "x-dead-letter-routing-key", dlqName },
            { "x-message-ttl", 36000000 }, // 10 hours
		});

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                if (model is not IModel localChannel)
                {
                    return;
                }

                var retryCount = this.GetRetryCount(ea.BasicProperties);
                var body = ea.Body.ToArray();

                var message = Encoding.UTF8.GetString(body);

                var transactionId = int.Parse(message);

                var isNotified = transactionId % 2 == 0;
                if (isNotified)
                {
                    localChannel.BasicAck(ea.DeliveryTag, false);
                }
                else
                {
                    if (retryCount < 2)
                    {
                        var delayMilliseconds = this.GetDelayInterval(retryCount);
                        var properties = this.CreateDelayedMessageProperties(localChannel, delayMilliseconds, retryCount);
                        localChannel.BasicPublish(string.Empty, queueName, properties, body);
                    }

                    // Acknowledge the original message
                    localChannel.BasicAck(ea.DeliveryTag, false);
                }
            };

            channel.BasicConsume(
                queue: queueName,
                autoAck: false,
                consumer: consumer);
        }

        private int GetRetryCount(IBasicProperties properties)
        {
            if (properties.Headers != null && properties.Headers.TryGetValue("x-retry-count", out var retryCountObj) && retryCountObj is int retryCount)
            {
                return retryCount;
            }

            return 0;
        }

        private IBasicProperties CreateDelayedMessageProperties(IModel channel, int delayMilliseconds, int retryCount)
        {
            var properties = channel.CreateBasicProperties();
            properties.Headers = new Dictionary<string, object>
        {
            { "x-delay", delayMilliseconds },
            { "x-retry-count", retryCount + 1 },
        };

            return properties;
        }

        private int GetDelayInterval(int retryCount)
        {
            return retryCount switch
            {
                0 => 120000, // 2 minutes
                1 => 600000, // 10 minutes
                _ => 36000000, // 10 hours
            };
        }
    }
}
