var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");
var postgresdb = postgres.AddDatabase("postgresdb");

var cache = builder.AddRedis("cache");

var rabbitmq = builder.AddRabbitMQ("messaging");

var server = builder.AddProject<Projects.Isekai_Server>("server")
    .WithReference(postgresdb)
    .WaitFor(postgresdb)
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints()
    .WithEndpoint("http",  e => e.Port = 5304)
    .WithEndpoint("https", e => e.Port = 7342);

builder.AddProject<Projects.Isekai_Worker>("worker")
    .WithReference(postgresdb)
    .WaitFor(postgresdb)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);

var webfrontend = builder.AddViteApp("webfrontend", "../frontend")
    .WithPnpm()
    .WithReference(server)
    .WaitFor(server)      .WithEnvironment("SERVER_HTTP", server.GetEndpoint("http"))
    .WithEnvironment("SERVER_HTTPS", server.GetEndpoint("https"))
    .WithEndpoint("http", e => e.Port = 5173);

server.PublishWithContainerFiles(webfrontend, "wwwroot");

builder.Build().Run();
