using System.Text.Json;
using System.Text.Json.Serialization;
using FloppyBot.Base.Logging;
using FloppyBot.Version;
using FloppyBot.WebApi.Agent.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Serilog;

// *** BOOT *****************************************************************************
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((ctx, lc) => lc
    .ConfigureSerilog(ctx.Configuration));

// *** SERVICES *************************************************************************
IServiceCollection services = builder.Services;
// - CORS
services
    .AddCors(o =>
    {
        o.AddDefaultPolicy(b => b
            .WithOrigins(builder.Configuration.GetSection("Cors").Get<string[]>())
            .WithMethods("GET", "POST", "PUT", "DELETE")
            .AllowAnyHeader()
            .AllowCredentials());
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
            }
        };
    });
services
    .AddAuthorization(opts =>
    {
        foreach (string permission in Permissions.AllPermissions)
        {
            opts.AddPermissionAsPolicy(permission);
        }
    })
    .AddSingleton<IAuthorizationHandler, HasPermissionHandler>();
// - Swagger
services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(o =>
    {
        o.SwaggerDoc(AboutThisApp.Info.Version, new OpenApiInfo
        {
            Title = "FloppyBot Management API",
            Version = AboutThisApp.Info.Version
        });
        o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Scheme = "Bearer",
            BearerFormat = "JWT",
            Type = SecuritySchemeType.Http,
            In = ParameterLocation.Header,
            Description = "JWT Token goes here"
        });
        o.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme
                    }
                },
                Array.Empty<string>()
            }
        });
    });
// - SignalR
services
    .AddSignalR();
// - Controllers
services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        // Enums should always be converted to String
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

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
        o.SwaggerEndpoint($"/swagger/{AboutThisApp.Info.Version}/swagger.json", "FloppyBot Management API");
        o.DocumentTitle = "FloppyBot Management API Explorer";
    });
}

// - CORS
app.UseCors();
// - Auth
app
    .UseAuthentication()
    .UseAuthorization();
// - Controllers
app.UseEndpoints(e => e.MapControllers());
// - SignalR
//app.MapHub<SoundCommandHub>("/hub/sound-command");

// *** START ****************************************************************************
app.Run();
