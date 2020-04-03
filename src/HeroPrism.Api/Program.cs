using System;
using System.Reflection;
using CorrelationId;
using HeroPrism.Api.Infrastructure;
using HeroPrism.Api.Infrastructure.Accessors;
using HeroPrism.Api.Infrastructure.Enrichers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace HeroPrism.Api
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var webHost = CreateWebHostBuilder(args).Build();

            return LogAndRun(webHost);
        }

        private static int LogAndRun(IHost webHost)
        {
            Log.Logger = BuildLogger(webHost);

            try
            {
                Log.Information("Starting application");
                webHost.Run();
                Log.Information("Stopped application");
                return 0;
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "Application terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IHostBuilder CreateWebHostBuilder(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(options => options.AddServerHeader = false);
                    webBuilder.UseStartup<Startup>();
                });

            return builder;
        }

        private static Logger BuildLogger(IHost webHost)
        {
            return new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithUserId(webHost.Services.GetRequiredService<HeroPrismSession>())
                .Enrich.WithCorrelationId(webHost.Services.GetRequiredService<ICorrelationContextAccessor>())
                .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] ({UserId}) ({CorrelationId}) [{SourceContext}] {Message}{NewLine}{Exception}")
                .WriteTo.ApplicationInsights(TelemetryConverter.Events)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .CreateLogger();
        }
    }
}