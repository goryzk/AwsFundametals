using Amazon.SimpleNotificationService;
using Customers.SnsPublisher.Api.Database;
using Customers.SnsPublisher.Api.Messaging;
using Customers.SnsPublisher.Api.Repositories;
using Customers.SnsPublisher.Api.Services;
using Customers.SnsPublisher.Api.Validation;
using Dapper;
using FluentValidation.AspNetCore;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory()
});

var config = builder.Configuration;
config.AddEnvironmentVariables("CustomersApi_");

builder.Services.AddFluentValidationAutoValidation(x =>
{
    x.DisableDataAnnotationsValidation = true;
}).AddFluentValidationClientsideAdapters();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

SqlMapper.AddTypeHandler(new GuidTypeHandler());
SqlMapper.RemoveTypeMap(typeof(Guid));
SqlMapper.RemoveTypeMap(typeof(Guid?));

builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
    new SqliteConnectionFactory(config.GetValue<string>("Database:ConnectionString")!));
builder.Services.AddSingleton<DatabaseInitializer>();

builder.Services.Configure<TopicSettings>(builder.Configuration.GetSection(nameof(TopicSettings)));
builder.Services.AddSingleton<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>();
builder.Services.AddSingleton<ISnsMessenger, SnsMessenger>();

builder.Services.AddSingleton<ICustomerRepository, CustomerRepository>();
builder.Services.AddSingleton<ICustomerService, CustomerService>();
builder.Services.AddSingleton<IGitHubService, GitHubService>();

builder.Services.AddHttpClient("GitHub", httpClient =>
{
    httpClient.BaseAddress = new Uri(config.GetValue<string>("GitHub:ApiBaseUrl")!);
    httpClient.DefaultRequestHeaders.Add(
        HeaderNames.Accept, "application/vnd.github.v3+json");
    httpClient.DefaultRequestHeaders.Add(
        HeaderNames.UserAgent, $"Course-{Environment.MachineName}");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseMiddleware<ValidationExceptionMiddleware>();
app.MapControllers();

var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

app.Run();
