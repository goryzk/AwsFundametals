using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using SnsPublisher;
using System.Text.Json;

var customer = new CustomerCreated
{
	DateOfBirth = DateTime.Now,
	Email = "gyezekyan13@gmail.com",
	FullName = "Gor Yezekyan",
	GitHubUsername  = "gor_yezekyan",
	ID = Guid.NewGuid(),
};

var snsClient = new AmazonSimpleNotificationServiceClient();

var topicArnResponse = await snsClient.FindTopicAsync("Customers");

var publishRequest = new PublishRequest
{
	TopicArn = topicArnResponse.TopicArn,
	Message = JsonSerializer.Serialize(customer),
	MessageAttributes = new Dictionary<string, MessageAttributeValue>
	{
		{
			"MessageType", new MessageAttributeValue
			{
				StringValue = typeof(CustomerCreated).Name,
				DataType = nameof(String),
			}
		}
	}
};

var response = await snsClient.PublishAsync(publishRequest);

Console.ReadKey();