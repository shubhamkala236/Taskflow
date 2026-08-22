namespace TaskFlow.Api.Services
{
	public interface IBlobSasService
	{
		Task EnsureContainerExistsAsync(CancellationToken cancellationToken = default);

		Uri GenerateUploadSasUri(string blobName, string contentType);
	}
}
