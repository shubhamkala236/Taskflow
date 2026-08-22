using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace TaskFlow.Api.Services
{
	public class BlobSasService : IBlobSasService
	{
		private readonly BlobContainerClient containerClient;
		private readonly StorageSharedKeyCredential sharedKeyCredential;

		public BlobSasService(BlobServiceClient blobServiceClient, IConfiguration configuration)
		{
			var containerName = configuration["BlobStorage:ContainerName"]
				?? throw new InvalidOperationException("Missing configuration: BlobStorage:ContainerName");
			containerClient = blobServiceClient.GetBlobContainerClient(containerName);

			var connectionString = configuration["BlobStorage:ConnectionString"]
				?? throw new InvalidOperationException("Missing configuration: BlobStorage:ConnectionString");
			sharedKeyCredential = GetSharedKeyCredential(connectionString);
		}

		public async Task EnsureContainerExistsAsync(CancellationToken cancellationToken = default)
		{
			await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
		}

		public Uri GenerateUploadSasUri(string blobName, string contentType)
		{
			var blobClient = containerClient.GetBlobClient(blobName);

			var sasBuilder = new BlobSasBuilder
			{
				BlobContainerName = containerClient.Name,
				BlobName = blobName,
				Resource = "b",
				ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(15),
				ContentType = contentType
			};
			sasBuilder.SetPermissions(BlobSasPermissions.Write | BlobSasPermissions.Create);

			var sasQueryParameters = sasBuilder.ToSasQueryParameters(sharedKeyCredential);

			var uriBuilder = new UriBuilder(blobClient.Uri)
			{
				Query = sasQueryParameters.ToString()
			};
			return uriBuilder.Uri;
		}

		private static StorageSharedKeyCredential GetSharedKeyCredential(string connectionString)
		{
			var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries)
				.Select(part => part.Split('=', 2))
				.Where(kv => kv.Length == 2)
				.ToDictionary(kv => kv[0], kv => kv[1], StringComparer.OrdinalIgnoreCase);

			var accountName = parts.GetValueOrDefault("AccountName")
				?? throw new InvalidOperationException("BlobStorage:ConnectionString is missing AccountName");
			var accountKey = parts.GetValueOrDefault("AccountKey")
				?? throw new InvalidOperationException("BlobStorage:ConnectionString is missing AccountKey");

			return new StorageSharedKeyCredential(accountName, accountKey);
		}
	}
}
