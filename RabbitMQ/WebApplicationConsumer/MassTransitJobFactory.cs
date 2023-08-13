using Quartz;
using Quartz.Spi;

namespace WebApplicationConsumer;

public class MassTransitJobFactory: IJobFactory
{
    private readonly IServiceScopeFactory scopeFactory;

    public MassTransitJobFactory(IServiceScopeFactory scopeFactory)
    {
        this.scopeFactory = scopeFactory;
    }

    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        using var scope = this.scopeFactory.CreateScope();
        return scope.ServiceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
    }

    public void ReturnJob(IJob job)
    {
        (job as IDisposable)?.Dispose();
    }
}