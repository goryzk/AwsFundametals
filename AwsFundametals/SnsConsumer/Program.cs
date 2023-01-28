using Amazon.SQS;
using Amazon.SQS.Model;

var queueName = args.Length == 1 ? args[0] : "Customers";

var cts = new CancellationTokenSource();

using var sqsClient = new AmazonSQSClient();

var queueUrlResponse = await sqsClient.GetQueueUrlAsync(queueName);

var receiveMessageRequest = new ReceiveMessageRequest
{
	QueueUrl = queueUrlResponse.QueueUrl,
	AttributeNames = new List<string> { "All" },
	MessageAttributeNames = new List<string> { "All" }
};

while (!cts.IsCancellationRequested)
{
	var response = await sqsClient.ReceiveMessageAsync(receiveMessageRequest, cts.Token);
	foreach (var message in response.Messages)
	{
		Console.WriteLine($"Message ID: {message.MessageId}");
		Console.WriteLine($"Message Body: {message.Body}");
		await sqsClient.DeleteMessageAsync(queueUrlResponse.QueueUrl, message.ReceiptHandle, cts.Token);
	}

	await Task.Delay(3000);
}
