using System.Text.Json;
using System.Text.Json.Serialization;
using FloppyBot.Base.Configuration;
using FloppyBot.Base.Cron;
using FloppyBot.Base.Logging;
using FloppyBot.Base.Storage.MongoDb;
using FloppyBot.Commands.Custom.Communication;
using FloppyBot.Commands.Registry;
using FloppyBot.Communication.Redis.Config;
using FloppyBot.HealthCheck.Core;
using FloppyBot.HealthCheck.KillSwitch;
using FloppyBot.HealthCheck.Receiver;
using FloppyBot.Version;
using FloppyBot.WebApi.Agent.Hubs;
using FloppyBot.WebApi.Auth;
using FloppyBot.WebApi.Base.ExceptionHandler;
using FloppyBot.WebApi.V1Compatibility;
using FloppyBot.WebApi.V1Compatibility.Mapping;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Serilog;

// *** BOOT *****************************************************************************
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.SetupEnvironmentConfig();
builder.Host.UseSerilog((ctx, lc) => lc.ConfigureSerilog(ctx.Configuration));

// *** SERVICES *************************************************************************
IServiceCollection services = builder.Services;

// - CORS
services.AddCors(o =>
{
    o.AddDefaultPolicy(b =>
        b.WithOrigins(
                builder.Configuration.GetSection("Cors").Get<string[]>() ?? Array.Empty<string>()
            )
            .WithMethods("GET", "POST", "PUT", "DELETE")
            .AllowAnyHeader()
            .AllowCredentials()
    );
});

// - Auth
services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        IConfigurationSection? configSection = builder.Configuration.GetSection("Jwt");
        o.Authority = configSection["Authority"];
        o.Audience = configSection["Audience"];
        o.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.Response.OnStarting(async () =>
                {
                    await context.Response.WriteAsync(
                        JsonSerializer.Serialize(new { Message = "You are not authorized!" })
                    );
                });

                return Task.CompletedTask;
            },
        };
    });
services
    .AddAuthorization(opts =>
    {
        foreach (string permission in Permissions.AllPermissions)
        {
            opts.AddPermissionAsPolicy(permission);
        }

        opts.AddPolicy("ApiKey", policy => policy.Requirements.Add(ApiKeyAuthRequirement.Instance));
    })
    .AddSingleton<IAuthorizationHandler, HasPermissionHandler>()
    .AddSingleton<IAuthorizationHandler, ApiKeyAuthHandler>();

// - Swagger
services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(o =>
    {
        o.SwaggerDoc(
            AboutThisApp.Info.Version,
            new OpenApiInfo
            {
                Title = "FloppyBot Management API",
                Version = AboutThisApp.Info.Version,
            }
        );
        o.AddSecurityDefinition(
            "Bearer",
            new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Scheme = "Bearer",
                BearerFormat = "JWT",
                Type = SecuritySchemeType.Http,
                In = ParameterLocation.Header,
                Description = "JWT Token goes here",
            }
        );
        o.AddSecurityRequirement(
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Id = "Bearer",
                            Type = ReferenceType.SecurityScheme,
                        },
                    },
                    Array.Empty<string>()
                },
            }
        );
    });

// - SignalR
services.AddSignalR();

// - Controllers
services
    .AddControllers(o =>
    {
        o.Filters.Add<GlobalExceptionHandler>();
    })
    .AddJsonOptions(o =>
    {
        // Enums should always be converted to String
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// - Dependencies
services
    .AddAutoMapper(typeof(V1CompatibilityProfile))
    .AddMongoDbStorage()
    .AddRedisCommunication()
    .AddDistributedCommandRegistry()
    .AddAuthDependencies()
    .AddCronJobSupport()
    .AddHealthCheck()
    .AddHealthCheckReceiver()
    .AddKillSwitchTrigger()
    .AddKillSwitch()
    .AddV1Compatibility()
    .AddSingleton<StreamSourceListener>()
    .AddMemoryCache();

// *** CONFIGURE ************************************************************************
var app = builder.Build();

// - Routing
app.UseRouting();

// - Swagger only in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o =>
    {
        o.SwaggerEndpoint(
            $"/swagger/{AboutThisApp.Info.Version}/swagger.json",
            "FloppyBot Management API"
        );
        o.DocumentTitle = "FloppyBot Management API Explorer";
    });
}

// - CORS
app.UseCors();

// - Auth
app.UseAuthentication().UseAuthorization();

// - Controllers
app.MapControllers();

// - SignalR
app.MapV1SignalRHub();
app.MapHub<StreamSourceHub>("/hub/stream-source");

// *** START ****************************************************************************
app.BootCronJobs()
    .ArmKillSwitch()
    .StartHealthCheckReceiver()
    .StartSoundCommandInvocationReceiver()
    .Do(host =>
    {
        // Start the StreamSourceListener
        host.Services.GetRequiredService<StreamSourceListener>();
    })
    .Run();
