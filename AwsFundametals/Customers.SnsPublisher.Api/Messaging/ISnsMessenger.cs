using Amazon.SimpleNotificationService.Model;

namespace Customers.SnsPublisher.Api.Messaging
{
	public interface ISnsMessenger
	{
		Task<PublishResponse> PublishMessageAsync<T>(T message);
	}
}
