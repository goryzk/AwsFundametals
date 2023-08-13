namespace WebApplicationConsumer;

public interface IPublishMessage<T>
where T : class
{
    Task PublishMessage(T message);
}