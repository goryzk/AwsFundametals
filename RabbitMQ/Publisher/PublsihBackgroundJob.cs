using Contracts;
using MassTransit;
using WebApplicationConsumer;

namespace Publisher;

public class PublsihBackgroundJob : BackgroundService
{
    private readonly IBus _publishMessage;

    public PublsihBackgroundJob(IBus publishMessage)
    {
        _publishMessage = publishMessage;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var rnd = new Random().Next(0, int.MaxValue);
            await _publishMessage.Publish(new TransactionIdPublisher(13, DateTime.Now), stoppingToken);
            await Task.Delay(1000, stoppingToken);
        }
    }
}