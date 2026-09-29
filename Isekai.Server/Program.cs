using DbUp;
using Isekai.Server.Repositories;
using Isekai.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.AddNpgsqlDataSource("postgresdb");

builder.Services.AddScoped<IShortUrlRepository, ShortUrlRepository>();
builder.Services.AddScoped<UrlShortenerService>();

var app = builder.Build();

var connectionString = builder.Configuration.GetConnectionString("postgresdb")
    ?? throw new InvalidOperationException("Connection string 'postgresdb' not found.");

EnsureDatabaseMigrated(connectionString);

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();
app.MapControllers();

app.UseFileServer();

app.Run();
return;

static void EnsureDatabaseMigrated(string connectionString)
{
    const int maxRetries = 5;
    var delay = TimeSpan.FromSeconds(2);

    for (var attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            var upgrader = DeployChanges.To
                .PostgresqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(System.Reflection.Assembly.GetExecutingAssembly())
                .LogToConsole()
                .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                throw new Exception("Database migration failed", result.Error);
            }
            return;
        }
        catch (Exception ex) when (attempt < maxRetries)
        {
            Console.WriteLine($"Migration attempt {attempt} failed: {ex.Message}. Retrying in {delay.TotalSeconds}s...");
            Thread.Sleep(delay);
            delay *= 2;
        }
    }

}
