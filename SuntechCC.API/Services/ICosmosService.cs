using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using SuntechCC.API.Models;
using SuntechCC.API.Options;

namespace SuntechCC.API.Services;

public interface ICosmosService
{
    Task InsertIntoCosmos(UserModel user);

    Task InitializeChangeFeed();

    Task DisposeChangeFeed();

    Task<Container> GetOrCreateContainer(string containerName);

    Task<Container> GetOrCreateLeaseContainer(string containerName);
}

public class CosmosService : ICosmosService
{
    private readonly CosmosClient _cosmosClient;
    private readonly CosmosOptions _options;

    private ChangeFeedProcessor _changeFeedProcessor;

    public CosmosService(CosmosClient cosmosClient, IOptions<CosmosOptions> options)
    {
        _cosmosClient = cosmosClient;
        _options = options.Value;
    }

    public async Task DisposeChangeFeed()
    {
        if (_changeFeedProcessor != null)
        {
            await _changeFeedProcessor.StopAsync();
        }
    }

    public async Task<Container> GetOrCreateContainer(string containerId)
    {
        var database = _cosmosClient.GetDatabase(_options.DatabaseId);
        var containerProperties = new ContainerProperties()
        {
            Id = containerId,
            PartitionKeyPath = "/email"
        };
        var throughput = ThroughputProperties.CreateManualThroughput(400);
        var container = await database.CreateContainerIfNotExistsAsync(containerProperties,throughput);
        return container;
    }

    private static Task ChangeHandler(IReadOnlyCollection<UserModel> changes, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task InitializeChangeFeed()
    {
        var container = await GetOrCreateContainer(_options.ContainerName);
        var leaseContainer = await GetOrCreateLeaseContainer(_options.LeaseContainerName);

        _changeFeedProcessor = container
            .GetChangeFeedProcessorBuilder<UserModel>(_options.ContainerName, ChangeHandler)
            .WithInstanceName(_options.InstanceName)
            .WithLeaseContainer(leaseContainer)
            .Build();

        await _changeFeedProcessor.StartAsync();
    }

    public async Task InsertIntoCosmos(UserModel user)
    {
        var container = await GetOrCreateContainer(_options.ContainerName);
        await container.UpsertItemAsync(user, partitionKey: new PartitionKey(user.Email));
    }

    public async Task<Container> GetOrCreateLeaseContainer(string containerName)
    {
        var database = _cosmosClient.GetDatabase(_options.DatabaseId);
        ContainerProperties leaseContainerProperties = new ContainerProperties(containerName, "/id");
        Container leaseContainer = await database.CreateContainerIfNotExistsAsync(leaseContainerProperties);

        return leaseContainer;
    }
}