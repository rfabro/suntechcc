using System;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SuntechCC.API;
using SuntechCC.API.Options;
using SuntechCC.API.Services;

[assembly: FunctionsStartup(typeof(Startup))]

namespace SuntechCC.API
{
    public class Startup : FunctionsStartup
    {
        private IConfiguration Configuration { get; set; }
        
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // configuration setup

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

            #region Cosmos

            services.AddSingleton<CosmosClient>((sp) =>
            {
                return new CosmosClientBuilder(Configuration["Azure:Cosmos:Connectionstring"])
                    .WithSerializerOptions(new CosmosSerializationOptions()
                    {
                        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                    }).Build();
            });

            services.Configure<CosmosOptions>(options =>
            {
                options.DatabaseId = Configuration["Azure:Cosmos:DatabaseId"];
                options.InstanceName = Configuration["Azure:Cosmos:InstanceName"];
                options.ContainerName = Configuration["Azure:Cosmos:Container"];
                options.LeaseContainerName = Configuration["Azure:Cosmos:LeaseContainer"];
            });

            #endregion

            services.AddScoped<ICosmosService, CosmosService>();
        }
    }
}