using System.Security.Claims;
using DbUp;
using Isekai.Data.Repositories;
using Isekai.Server.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();


builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor
                         | ForwardedHeaders.XForwardedProto
                         | ForwardedHeaders.XForwardedHost;
    o.KnownIPNetworks.Clear();   // trust the proxy/container network; tighten for prod
    o.KnownProxies.Clear();
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    //options.DefaultChallengeScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;
    // TODO: Research what is challenge scheme
})
.AddCookie(options =>
{
    options.Cookie.Name = "app.auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax; // Required for google redirect
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;

    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? throw new InvalidOperationException();
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? throw new InvalidOperationException();
    options.ClaimActions.MapJsonKey("picture", "picture");
    
    // TODO: Research AddGoogle vs AddGoogleOpenIdConnect
    // Default response_mode is form_post: a cross-site POST from Google. SameSite=Lax
    // cookies are not sent on that POST, so correlation fails. Query is a top-level GET,
    // which does include Lax cookies. The shared-framework correlation cookie defaults to
    // Secure, which browsers drop on this HTTP endpoint, so match the request scheme.
    //.ResponseMode = OpenIdConnectResponseMode.Query;
    //options.NonceCookie.SameSite = SameSiteMode.Lax;
    //options.NonceCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    //options.CorrelationCookie.SameSite = SameSiteMode.Lax;
    //options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

    options.Events.OnCreatingTicket = async context =>
    {
        var provider = context.Scheme.Name;  // "Google", "GitHub", ...
        var subject  = context.Principal!.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var name     = context.Principal.FindFirstValue(ClaimTypes.Name);
        var email    = context.Principal.FindFirstValue(ClaimTypes.Email);

        var svc = context.HttpContext.RequestServices.GetRequiredService<IAccountService>();
        var userId = await svc.GetOrCreateUserAsync(provider.ToLowerInvariant(), subject, name, email);

        // Replace the principal with one carrying YOUR id
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, name ?? ""),
                new Claim(ClaimTypes.Email, email ?? ""),
            }, 
            CookieAuthenticationDefaults.AuthenticationScheme);

        context.Principal = new ClaimsPrincipal(identity);
    };
});

builder.Services.AddAuthorization();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.AddNpgsqlDataSource("postgresdb");

builder.AddRedisClient(connectionName: "cache");

builder.AddRabbitMQClient(connectionName: "messaging");

builder.Services.AddSingleton<IShortUrlRepository, ShortUrlRepository>();
builder.Services.AddSingleton<IAccountRepository, AccountRepository>();
builder.Services.AddSingleton<UrlShortenerService>();
builder.Services.AddSingleton<IAccountService, AccountService>();

var app = builder.Build();

var connectionString = builder.Configuration.GetConnectionString("postgresdb")
    ?? throw new InvalidOperationException("Connection string 'postgresdb' not found.");

EnsureDatabaseMigrated(connectionString);

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// TODO: Research this
app.UseForwardedHeaders(); 
app.UseAuthentication();
app.UseAuthorization();

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
