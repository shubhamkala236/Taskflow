using Microsoft.Azure.Cosmos;
using TaskFlow.Api.Activity;

namespace TaskFlow.Api.Services
{
	public class CosmosActivityLogService : IActivityLogService
	{
		private readonly CosmosClient cosmosClient;
		private readonly string databaseName;
		private readonly string containerName;
		private Container? container;

		public CosmosActivityLogService(CosmosClient cosmosClient, IConfiguration configuration)
		{
			this.cosmosClient = cosmosClient;
			databaseName = configuration["CosmosDb:DatabaseName"]
				?? throw new InvalidOperationException("Missing configuration: CosmosDb:DatabaseName");
			containerName = configuration["CosmosDb:ContainerName"]
				?? throw new InvalidOperationException("Missing configuration: CosmosDb:ContainerName");
		}

		public async Task EnsureDatabaseAndContainerExistsAsync(CancellationToken cancellationToken = default)
		{
			var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName, cancellationToken: cancellationToken);
			var containerResponse = await database.Database.CreateContainerIfNotExistsAsync(
				containerName,
				partitionKeyPath: "/tenantId",
				cancellationToken: cancellationToken);
			container = containerResponse.Container;
		}

		public async Task LogAsync(ActivityLogEntry entry, CancellationToken cancellationToken = default)
		{
			var target = container ?? cosmosClient.GetContainer(databaseName, containerName);
			await target.CreateItemAsync(entry, new PartitionKey(entry.TenantId), cancellationToken: cancellationToken);
		}
	}
}
