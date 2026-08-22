namespace TaskFlow.Api.Entities
{
	public class TaskAttachment
	{
		public Guid Id { get; set; }

		public Guid TaskItemId { get; set; }

		public TaskItem? TaskItem { get; set; }

		public string BlobName { get; set; } = string.Empty;

		public string FileName { get; set; } = string.Empty;

		public string ContentType { get; set; } = string.Empty;

		public long SizeBytes { get; set; }

		public DateTimeOffset UploadedAt { get; set; }

		public bool Confirmed { get; set; }
	}
}
