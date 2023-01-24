using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Customers.Api.Messaging
{
	public class SqsMessenger : ISqsMessenger
	{
		private readonly IAmazonSQS _sqs;
		private readonly IOptions<QueueSettings> _queueSettings;
		private string? _queueURL;

		public SqsMessenger(IAmazonSQS sqs, IOptions<QueueSettings> queueSettings)
		{
			_sqs = sqs;
			_queueSettings = queueSettings;
		}

		public async Task<SendMessageResponse> SendMessageAsync<T>(T message)
		{
			var queueURL = await GetQueueUrlAsync();

			var sendMessageRequest = new SendMessageRequest
			{
				QueueUrl = queueURL,
				MessageBody = JsonSerializer.Serialize(message),
				MessageAttributes = new Dictionary<string, MessageAttributeValue>
				{
					{
						"MessageType", new MessageAttributeValue
						{
							DataType = nameof(String),
							StringValue = typeof(T).Name
						}
					}
				}
			};

			return await _sqs.SendMessageAsync(sendMessageRequest);
		}

		private async Task<string> GetQueueUrlAsync()
		{
			if (_queueURL is not null)
			{
				return _queueURL;
			}

			var queueUrlResponse = await _sqs.GetQueueUrlAsync(_queueSettings.Value.QueueName);
			_queueURL= queueUrlResponse.QueueUrl;
			return _queueURL;
		}
	}
}
