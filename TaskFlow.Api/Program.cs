using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Scalar.AspNetCore;
using TaskFlow.Api.Data;
using TaskFlow.Api.Services;
using TaskFlow.Api.Tenancy;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddAuthorization();
builder.Services.AddScoped<ITenantProvider, TenantProvider>();

const string AllowSwaPolicy = "AllowSwa";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy(AllowSwaPolicy, policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<TaskFlowDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.Services.AddSingleton(_ => new BlobServiceClient(builder.Configuration["BlobStorage:ConnectionString"]));
builder.Services.AddScoped<IBlobSasService, BlobSasService>();

builder.Services.AddSingleton(_ => new CosmosClient(
    builder.Configuration["CosmosDb:ConnectionString"],
    new CosmosClientOptions
    {
        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    }));
builder.Services.AddScoped<IActivityLogService, CosmosActivityLogService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    services.GetRequiredService<TaskFlowDbContext>().Database.Migrate();
    await services.GetRequiredService<IBlobSasService>().EnsureContainerExistsAsync();
    await services.GetRequiredService<IActivityLogService>().EnsureDatabaseAndContainerExistsAsync();
}

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();

app.UseCors(AllowSwaPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/healthz");

app.Run();
