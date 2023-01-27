using Amazon.SQS;
using Amazon.SQS.Model;
using SqsPublisher;
using System.Text.Json;

using var sqsClient = new AmazonSQSClient();

var customer = new CustomerCreated
{
	DateOfBirth = DateTime.Now,
	Email = "gor.yezekyan@digitain.com",
	FullName = "Gor Yezekyan",
	GitHubUsername = "gyzk",
	ID = Guid.NewGuid(),
};

var queueUrlResponse = await sqsClient.GetQueueUrlAsync("Customers");

var sendMessageRequest = new SendMessageRequest
{
	QueueUrl = queueUrlResponse.QueueUrl,
	MessageBody = JsonSerializer.Serialize(customer),
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

var response = await sqsClient.SendMessageAsync(sendMessageRequest);

Console.ReadKey();

