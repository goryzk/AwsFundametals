using Contracts;
using MassTransit;
using MassTransit.QuartzIntegration;
using Quartz;

namespace WebApplicationConsumer;

public class MassTransitShchedulerConsumer : IConsumer<TransactionIdPublisher>
{
    private readonly ISomeService someService;
    private readonly ILogger<MassTransitShchedulerConsumer> logger;
    private readonly IScheduler _schedulerFactory;

    public MassTransitShchedulerConsumer(ISomeService someService, ILogger<MassTransitShchedulerConsumer> logger, IScheduler scheduler)
    {
        this.someService = someService;
        this.logger = logger;
        _schedulerFactory = scheduler;
    }

    public async Task Consume(ConsumeContext<TransactionIdPublisher> context)
    {
        try
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
                    $"{transactionId} Still in the queue Date:{DateTime.Now}, count:{context.GetRetryCount() + 1}");
                
                throw new Exception("Very bad thing happened");

            }
        }
        catch (Exception e)
        {
            var redeliverCount = context.Headers.Get("MT-Redelivery-Count", "0");
            if (int.TryParse(redeliverCount, out var n) && n <= 10)
            {
                var jobKey = new JobKey(Guid.NewGuid().ToString());
                var jobDetail = JobBuilder.Create<MassTransitScheduledMessageJob>()
                    .WithIdentity(jobKey)
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithIdentity(Guid.NewGuid().ToString())
                    .StartAt(DateTime.Now.AddSeconds(5)) // Set your desired delay here
                    .Build();

                jobDetail.JobDataMap["message"] = context.Message.id;
                await _schedulerFactory.ScheduleJob(jobDetail, trigger);
            }
            else if (n is > 10 and <= 14)
            {
                var jobKey = new JobKey(Guid.NewGuid().ToString());
                var jobDetail = JobBuilder.Create<MassTransitScheduledMessageJob>()
                    .WithIdentity(jobKey)
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithIdentity(Guid.NewGuid().ToString())
                    .StartAt(DateTime.Now.AddSeconds(5)) // Set your desired delay here
                    .Build();

                jobDetail.JobDataMap["message"] = context.Message.id;
                await _schedulerFactory.ScheduleJob(jobDetail, trigger);
            }
        }
    }
}