using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PnP.Core.Auth;
using PnP.Core.Services.Builder.Configuration;
using SharepointDatalakeConnector.Service.ConfigModels;
using SharepointDatalakeConnector.Service.Interfaces;
using SharepointDatalakeConnector.Service.Services;
using SharepointDatalakeFunction;
using System;
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

            var config = builder.GetContext().Configuration;
            var azureFunctionSettings = new AzureFunctionSettings();
            config.Bind(azureFunctionSettings);

            builder.Services.AddPnPCore(options =>
            {
                // Disable telemetry because of mixed versions on AppInsights dependencies
                options.DisableTelemetry = true;

                // Configure an authentication provider with certificate (Required for app only)
                var authProvider = new X509CertificateAuthenticationProvider(azureFunctionSettings.ClientId,
                    azureFunctionSettings.TenantId,
                    StoreName.My,
                    StoreLocation.CurrentUser,
                    azureFunctionSettings.CertificateThumbprint);
                // And set it as default
                options.DefaultAuthenticationProvider = authProvider;

                // Add a default configuration with the site configured in app settings
                options.Sites.Add("Default",
                       new PnP.Core.Services.Builder.Configuration.PnPCoreSiteOptions
                       {
                           SiteUrl = azureFunctionSettings.SiteUrl,
                           AuthenticationProvider = authProvider
                       });
            });

        }

    }
}
