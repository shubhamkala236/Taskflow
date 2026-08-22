namespace TaskFlow.Api.Dtos
{
	public record TaskDto(
		Guid Id,
		string Title,
		string? Description,
		bool IsComplete,
		DateTimeOffset CreatedAt,
		IReadOnlyList<AttachmentDto> Attachments);

	public record AttachmentDto(
		Guid Id,
		string FileName,
		string ContentType,
		long SizeBytes,
		DateTimeOffset UploadedAt);

	public record CreateTaskRequest(string Title, string? Description);

	public record UpdateTaskRequest(string Title, string? Description, bool IsComplete);

	public record RequestUploadSasRequest(string FileName, string ContentType);

	public record RequestUploadSasResponse(Guid AttachmentId, string UploadUrl, string BlobName);
}
