using Amazon.SQS;
using Amazon.SQS.Model;
using Customers.Consumer.Messages;
using MediatR;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Customers.Consumer
{
    public class QueueConsumerService : BackgroundService
	{
		private readonly IAmazonSQS _amazonSQS;
		private readonly IOptions<QueueSettings> _options;
		private readonly IMediator _mediatr;
		private readonly ILogger<QueueConsumerService> _logger;

		public QueueConsumerService(IAmazonSQS amazonSQS, IOptions<QueueSettings> options, IMediator mediator, ILogger<QueueConsumerService> logger)
		{
			_amazonSQS = amazonSQS;
			_options = options;
			_mediatr = mediator;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var queueUrlResponse = await _amazonSQS.GetQueueUrlAsync("Customers", stoppingToken);

			var receiveMessageRequest = new ReceiveMessageRequest
			{
				QueueUrl = queueUrlResponse.QueueUrl,
				AttributeNames = new List<string> { "All" },
				MessageAttributeNames = new List<string> { "All" },
				MaxNumberOfMessages = 1
			};

			while (!stoppingToken.IsCancellationRequested)
			{
				var response = await _amazonSQS.ReceiveMessageAsync(receiveMessageRequest, stoppingToken);
				foreach (var message in response.Messages)
				{
					var messageType = message.MessageAttributes["MessageType"].StringValue;

					var type = Type.GetType($"Customers.Consumer.Messages.{messageType}");

					if (type is null)
					{
						_logger.LogWarning("Unknown message type: {MessageType}", messageType);
						continue;
					}

					var typedMessage = (ISqsMessage)JsonSerializer.Deserialize(message.Body, type);

					try
					{
						await _mediatr.Send(typedMessage, stoppingToken);
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "Message failed during processing");
						continue;
					}

					await _amazonSQS.DeleteMessageAsync(queueUrlResponse.QueueUrl, message.ReceiptHandle);
				}

				await Task.Delay(3000, stoppingToken);
			}
		}
	}
}
