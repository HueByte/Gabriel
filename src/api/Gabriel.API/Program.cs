using Gabriel.API.Configuration;
using Gabriel.API.Identity;
using Gabriel.API.Middleware;
using Gabriel.Core;
using Gabriel.Core.Identity;
using Gabriel.Infrastructure;
using Gabriel.Infrastructure.Identity;
using Gabriel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Pull secrets from Infisical before service registration so IOptions bindings
// (e.g. GrokOptions reading Providers:Grok:ApiKey, JwtOptions reading Jwt:SigningKey)
// see the live values.
builder.Configuration.AddInfisical(opts =>
    builder.Configuration.GetSection(InfisicalOptions.SectionName).Bind(opts));

builder.Services.AddOptions<InfisicalOptions>()
    .Bind(builder.Configuration.GetSection(InfisicalOptions.SectionName));

const string CorsPolicy = "WebApp";

builder.Services.AddControllers(opts =>
{
    // Every controller route is auto-prefixed with /api — no per-controller [Route("api/...")] needed.
    opts.Conventions.Insert(0, new GlobalRoutePrefixConvention("api"));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SupportNonNullableReferenceTypes();
    c.SchemaFilter<RequireNonNullablePropertiesSchemaFilter>();
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddCors(opts =>
{
    var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    opts.AddPolicy(CorsPolicy, p => p
        .WithOrigins(origins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        // Required so the browser sends the Identity auth cookie on cross-origin
        // fetches (non-issue when running through the Vite proxy, but defaults are
        // brittle once a different origin is in play).
        .AllowCredentials());
});

builder.Services.AddCoreServices(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

// Identity + JWT — three auth schemes wired here; see IdentityServiceCollectionExtensions.
builder.Services.AddIdentityAndAuth(builder.Configuration);

// ICurrentUser pulls from HttpContext.User regardless of which scheme authenticated.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

var app = builder.Build();

if (Environment.GetEnvironmentVariable("SKIP_DB_INIT") != "true")
{
    using var scope = app.Services.CreateScope();
    var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await ctx.Database.MigrateAsync();
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c => c.RouteTemplate = "api/swagger/{documentName}/swagger.json");
    app.UseSwaggerUI(c =>
    {
        c.RoutePrefix = "api/swagger";
        c.SwaggerEndpoint("/api/swagger/v1/swagger.json", "v1");
    });
}

app.UseCors(CorsPolicy);

// Auth middleware must run before MapControllers / MapIdentityApi so endpoints
// see the authenticated principal.
app.UseAuthentication();
app.UseAuthorization();

// Auth endpoints are controller-based (see AuthController) — no MapIdentityApi.
// The webapp authenticates via /api/auth/login which sets HttpOnly JWT cookies;
// JwtBearer's OnMessageReceived event reads the access cookie back on every request.

app.MapControllers();

app.Run();
