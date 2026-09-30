using Isekai.Data.Repositories;
using Isekai.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.AddNpgsqlDataSource("postgresdb");
builder.AddRabbitMQClient("messaging");
builder.Services.AddSingleton<IShortUrlRepository, ShortUrlRepository>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();