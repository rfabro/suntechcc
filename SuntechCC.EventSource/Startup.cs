using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SuntechCC.EventSource;

[assembly: FunctionsStartup(typeof(Startup))]

namespace SuntechCC.EventSource
{
    public class Startup : FunctionsStartup
    {
        private IConfiguration Configuration { get; set; }
        
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // setup configuration
            #region Setup Configuration
            var services = builder.Services;

            var executionContextOptions = services
                .BuildServiceProvider()
                .GetService<IOptions<ExecutionContextOptions>>()?
                .Value;

            if (executionContextOptions == null)
            {
                throw new Exception("Can't load current execution context");
            }

            Configuration = new ConfigurationBuilder()
                .SetBasePath(executionContextOptions.AppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            #endregion
        }
    }
}