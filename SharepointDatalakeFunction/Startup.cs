using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PnP.Core.Auth;
using PnP.Core.Services.Builder.Configuration;
using SharepointDatalakeConnector.Service.ConfigModels;
using SharepointDatalakeConnector.Service.Interfaces;
using SharepointDatalakeConnector.Service.Services;
using SharepointDatalakeFunction;
using System;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

[assembly: WebJobsStartup(typeof(Startup))]
namespace SharepointDatalakeFunction
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.TryAddScoped<ISharepointService, SharepointService>();

            builder.Services.TryAddScoped<IDatalakeService, DatalakeService>();

            builder.Services.TryAddScoped<ISqlService, SqlService>();

            builder.Services.AddOptions<DataLakeSettings>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection("DataLakeSettings").Bind(settings);
                });

            builder.Services.AddOptions<SharepointSettings>()
               .Configure<IConfiguration>((settings, configuration) =>
               {
                   configuration.GetSection("SharepointSettings").Bind(settings);
               });

            builder.Services.AddOptions<DatabaseSettings>()
               .Configure<IConfiguration>((settings, configuration) =>
               {
                   configuration.GetSection("DatabaseSettings").Bind(settings);
               });

            

            builder.Services.AddPnPCore(options =>
            {
                // Disable telemetry because of mixed versions on AppInsights dependencies
                options.DisableTelemetry = true;

                // Configure an authentication provider with certificate (Required for app only)
                var authProvider = new X509CertificateAuthenticationProvider(Environment.GetEnvironmentVariable("SharepointSettings:ClientId"),
                    Environment.GetEnvironmentVariable("SharepointSettings:TenantId"),
                    StoreName.My,
                    StoreLocation.CurrentUser,
                    Environment.GetEnvironmentVariable("SharepointSettings:CertificateThumbprint"));
                // And set it as default
                options.DefaultAuthenticationProvider = authProvider;

                // Add a default configuration with the site configured in app settings
                options.Sites.Add("Default",
                       new PnP.Core.Services.Builder.Configuration.PnPCoreSiteOptions
                       {
                           SiteUrl = Environment.GetEnvironmentVariable("SharepointSettings:SiteUrl"),
                           AuthenticationProvider = authProvider
                       });
            });
        }

    }
}
