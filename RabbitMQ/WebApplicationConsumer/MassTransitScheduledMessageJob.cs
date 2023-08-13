using Contracts;
using MassTransit;
using Quartz;

namespace WebApplicationConsumer;

public class MassTransitScheduledMessageJob: IJob
{
    //private readonly IBus bus;
    //
    //public MassTransitScheduledMessageJob(IBus bus)
    //{
    //    this.bus = bus;
    //}

    public async Task Execute(IJobExecutionContext context)
    {
        if (context.JobDetail.JobDataMap["message"] is string messageContent)
        {
            //await bus.Publish(new TransactionIdPublisher(int.Parse(messageContent), DateTime.Now));
        }
    }
}