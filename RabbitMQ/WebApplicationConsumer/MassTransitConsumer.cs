using System.Globalization;
using Contracts;
using MassTransit;

namespace WebApplicationConsumer;

public class MassTransitConsumer : IConsumer<TransactionIdPublisher>
{
    private readonly ISomeService someService;
    private readonly ILogger<MassTransitConsumer> logger;
    private readonly IBus bus;

    public MassTransitConsumer(ISomeService someService, ILogger<MassTransitConsumer> logger, IBus bus)
    {
        this.someService = someService;
        this.logger = logger;
        this.bus = bus;
    }

    public async Task Consume(ConsumeContext<TransactionIdPublisher> context)
    {
        var transactionId = context.Message.id;

        var result = await this.someService.CheckData(transactionId);

        if (result)
        {
            // Acknowledge the message
            this.logger.LogInformation($"{transactionId} Removed from queue, Date:{DateTime.Now}");
        }
        else
        {
            this.logger.LogInformation(
                $"{transactionId} Still in the queue Date:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}, count:{context.GetRetryCount()}");
                
            throw new Exception("Very bad thing happened");

        }
        
        //try
        //{
        //    var transactionId = context.Message.id;
        //
        //    var result = await this.someService.CheckData(transactionId);
        //
        //    if (result)
        //    {
        //        // Acknowledge the message
        //        this.logger.LogInformation($"{transactionId} Removed from queue, Date:{DateTime.Now}");
        //    }
        //    else
        //    {
        //        this.logger.LogInformation(
        //            $"{transactionId} Still in the queue Date:{DateTime.Now}, count:{context.GetRetryCount() + 1}");
        //        
        //        throw new Exception("Very bad thing happened");
        //
        //    }
        //}
        //catch (Exception e)
        //{
        //    //var redeliverCount = context.Headers.Get("MT-Redelivery-Count", "-1");
        //    //if (int.TryParse(redeliverCount, out var n) && n <= 10)
        //    //{
        //    //    await context.Redeliver(TimeSpan.FromMinutes(5));
        //    //}
        //    //else if (n is > 10 and <= 14)
        //    //{
        //    //    await context.Redeliver(TimeSpan.FromMinutes(15));
        //    //}
        //}
    }
}