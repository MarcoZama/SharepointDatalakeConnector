using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PnP.Core.Services.Builder.Configuration;
using SharepointDatalakeConnector.ConfigModels;
using SharepointDatalakeConnector.Service;
using System;

[assembly: WebJobsStartup(typeof(SharepointDatalakeConnector.Startup))]
namespace SharepointDatalakeConnector
{
    public class Startup : IWebJobsStartup
    {       
        public void Configure(IWebJobsBuilder builder)
        {

            builder.Services.TryAddScoped<ISharepointDatalakeService, SharepointDatalakeService>();

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

            //builder.Services.AddPnPCore(options =>
            //{
            //    // Disable telemetry because of mixed versions on AppInsights dependencies
            //    options.DisableTelemetry = true;

            //    var authProvider = new ManagedIdentityTokenProvider();

            //    // And set it as default
            //    options.DefaultAuthenticationProvider = authProvider;

            //    // Add a default configuration with the site configured in app settings
            //    options.Sites.Add("Default",
            //        new PnPCoreSiteOptions
            //        {
            //            SiteUrl = Environment.GetEnvironmentVariable("SiteUrl"),
            //            AuthenticationProvider = authProvider
            //        });
            //});

        }
      
    }
}
