using Contracts;
using MassTransit;
using MassTransit.QuartzIntegration;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using WebApplicationConsumer;
using MassTransitJobFactory = WebApplicationConsumer.MassTransitJobFactory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ISomeService, SomeService>();

//builder.Services.AddSingleton<MassTransitJobFactory>();
//builder.Services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

//builder.Services.AddScoped<MassTransitScheduledMessageJob>();
//builder.Services.AddQuartz(q =>
//{
//    q.UseJobFactory<MassTransitJobFactory>();
//    q.UseMicrosoftDependencyInjectionJobFactory();
//});

//builder.Services.AddSingleton<ISchedulerFactory>(new StdSchedulerFactory());
//builder.Services.AddSingleton<IScheduler>(provider =>
//{
//    var schedulerFactory = provider.GetRequiredService<ISchedulerFactory>();
//    return schedulerFactory.GetScheduler().Result;
//});


var rabbitMQSettings = builder.Configuration.GetSection("RabbitMQ");
var hostName = rabbitMQSettings["Host"];
var port = int.Parse(rabbitMQSettings["Port"]);
var userName = rabbitMQSettings["UserName"];
var password = rabbitMQSettings["Password"];

var retryPolicies = new List<(Type exceptionType, int maxRetries, TimeSpan interval)>
{
    (typeof(FirstException), 10, TimeSpan.FromSeconds(5)), // First policy
    (typeof(SecondException), 5, TimeSpan.FromSeconds(10)) // Second policy
};

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.SetInMemorySagaRepositoryProvider();
    //x.AddConsumer(typeof(MassTransitShchedulerConsumer));
    x.AddConsumer(typeof(MassTransitConsumer));
    
    //x.AddMessageScheduler(new Uri($"queue:{nameof(TransactionIdPublisher)}-Queue"));
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(hostName, "/", h =>
        {
            h.Username(userName);
            h.Password(password);
        });
        
        //cfg.UsePublishMessageScheduler();
        //cfg.UseMessageScheduler(new Uri($"queue:{nameof(TransactionIdPublisher)}-Queue"));
        
        cfg.ReceiveEndpoint("TransactionIdPublisher-Queue" ,ep =>
        {
            ep.UseFilter(f =>
            {
                f.
            });
            ep.DiscardFaultedMessages();
            //var quartzScheduler = CreateScheduler(builder).Result;
            //var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<MassTransitShchedulerConsumer>>();
            //var service = builder.Services.BuildServiceProvider().GetRequiredService<ISomeService>();

            //quartzScheduler.Start();
            
            //ep.Consumer(() => new MassTransitShchedulerConsumer(service, logger, quartzScheduler));
            
            ep.Consumer<MassTransitConsumer>(context, factory =>
            {
                factory.UseMessageRetry(mr =>
                {
                    mr.Interval(5, TimeSpan.FromSeconds(5)).Handle<Exception>();
                });
            });
            ep.ConfigureConsumeTopology = false;
            ep.Bind("TransactionIdPublisher-Queue_exchange", b => b.RoutingKey = "TransactionIdPublisher-Queue_routing_key");
        });

        cfg.ConfigureEndpoints(context);
    });
});

static async Task<IScheduler> CreateScheduler(WebApplicationBuilder builder)
{
    var schedulerFactory = new StdSchedulerFactory();
    var scheduler = await schedulerFactory.GetScheduler();
    
    using (var scope = builder.Services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>().CreateScope())
    {
        scheduler.JobFactory = scope.ServiceProvider.GetRequiredService<MassTransitJobFactory>();
    }
    
    return scheduler;
}

bool IsInRetryPolicies(Exception exception)
{
    foreach (var (exceptionType, maxRetries, interval) in retryPolicies)
    {
        if (exceptionType.IsInstanceOfType(exception))
        {
            // Apply the retry policy with maxRetries and interval
            // You can also handle moving to skipped queue here if needed
            return true;
        }
    }
    
    return false;
}



builder.Services.AddLogging();

var app = builder.Build();

app.Run();
