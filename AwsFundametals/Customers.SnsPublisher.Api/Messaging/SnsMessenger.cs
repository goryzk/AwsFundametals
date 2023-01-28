using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Customers.SnsPublisher.Api.Messaging
{
    public class SnsMessenger : ISnsMessenger
    {
        private readonly IAmazonSimpleNotificationService _sns;
        private readonly IOptions<TopicSettings> _topicSettings;
        private string? _topicARN;

        public SnsMessenger(IAmazonSimpleNotificationService sns, IOptions<TopicSettings> topicSettings)
        {
            _sns = sns;
            _topicSettings = topicSettings;
        }

        public async Task<PublishResponse> PublishMessageAsync<T>(T message)
        {
            var topicARN = await GetTopicArnAsync();

            var publishRequest = new PublishRequest
            {
                TopicArn = topicARN,
                Message = JsonSerializer.Serialize(message),
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {
                        "MessageType", new MessageAttributeValue
                        {
                            StringValue = typeof(T).Name,
                            DataType = nameof(String),
                        }
                    }
                }
            };

            return await _sns.PublishAsync(publishRequest);
        }

        private async ValueTask<string> GetTopicArnAsync()
        {
            if (_topicARN is not null)
            {
                return _topicARN;
            }

            var topic = await _sns.FindTopicAsync(_topicSettings.Value.TopicName);
            _topicARN = topic.TopicArn;

            return _topicARN;
        }
    }
}
