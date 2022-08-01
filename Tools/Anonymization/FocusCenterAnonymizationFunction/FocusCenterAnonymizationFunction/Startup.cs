using AnonymizationFunctionApp.Commands;
using AnonymizationFunctionApp.Interfaces;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

[assembly: FunctionsStartup(typeof(AnonymizationFunctionApp.Startup))]

namespace AnonymizationFunctionApp
{
    public class Startup : FunctionsStartup
    {
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            FunctionsHostBuilderContext context = builder.GetContext();

            builder.ConfigurationBuilder
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), optional: true, reloadOnChange: false)
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"appsettings.{context.EnvironmentName}.json"), optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();

            builder.ConfigurationBuilder.Build();
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            //commands
            builder.Services.AddTransient<IAnonymizationCommand, AnonymizationCommand>();

            //managers
            builder.Services.AddScoped<ICrmConnectionManager, CrmConnectionManager>();
            builder.Services.AddScoped<IBulkRequestManager, BulkRequestManager>();
        }
    }
}
