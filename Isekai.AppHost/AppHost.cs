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
    .WithExternalHttpEndpoints();

var webfrontend = builder.AddViteApp("webfrontend", "../frontend", "pnpm")
    .WithReference(server)
    .WaitFor(server);

server.PublishWithContainerFiles(webfrontend, "wwwroot");

builder.Build().Run();
