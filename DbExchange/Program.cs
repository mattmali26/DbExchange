using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;

namespace DbExchange
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var host = AppStartup();

            try
            {
                var workflowManager = ActivatorUtilities.GetServiceOrCreateInstance<WorkflowManager>(host.Services);
                workflowManager.Process();
            }
            catch (Exception ex)
            {
                var logger = ActivatorUtilities.GetServiceOrCreateInstance<ILogger<Program>>(host.Services);
                logger.LogError(ex, "Something wrong");
            }
        }

        private static IHost AppStartup()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", false, true)
                    .AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    var fileConfig = context.Configuration.GetSection(nameof(FileQuerySettings));
                    services.Configure<FileQuerySettings>(fileConfig);

                    var connectionStringSection = context.Configuration.GetSection("ConnectionStringList");
                    services.Configure<List<ConnectionStringOption>>(connectionStringSection);

                    services.AddSingleton<ConnectionsManager>();

                    services.AddScoped<WorkflowManager>();
                })
                .UseSerilog((hostingContext, loggerConfiguration) =>
                {
                    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
                })
                .Build();

            return host;
        }
    }
}