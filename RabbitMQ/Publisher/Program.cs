using Contracts;
using MassTransit;
using MassTransit.Transports.Fabric;
using Microsoft.AspNetCore.Mvc;
using Publisher;
using WebApplicationConsumer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<PublsihBackgroundJob>();

var rabbitMQSettings = builder.Configuration.GetSection("RabbitMQ");
var hostName = rabbitMQSettings["Host"];
var port = int.Parse(rabbitMQSettings["Port"]);
var userName = rabbitMQSettings["UserName"];
var password = rabbitMQSettings["Password"];

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.SetInMemorySagaRepositoryProvider();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(hostName, "/", h =>
        {
            h.Username(userName);
            h.Password(password);
        });
        
        cfg.Publish<TransactionIdPublisher>(p =>
        {
            p.ExchangeType = "direct";
        });
        
        ////cfg.ReceiveEndpoint("TransactionIdPublisher-Queue" ,ep =>
        ////{
        ////    ep.Durable = true;
        ////    ep.AutoDelete = false;
        ////    ep.Exclusive = false;
        ////    ep.ConfigureConsumeTopology = false; // Disable auto-configure
        ////    ep.Bind("TransactionIdPublisher-Queue_exchange", b => b.RoutingKey = "TransactionIdPublisher-Queue_routing_key");
        ////    ep.Bind<TransactionIdPublisher>(c =>
        ////    {
        ////        c.ExchangeType = "direct";
        ////    });
        ////    
        ////});

        cfg.ConfigureEndpoints(context);
    });});

builder.Services.AddLogging();

var app = builder.Build();
app.Run();