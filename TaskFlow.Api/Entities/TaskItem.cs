namespace TaskFlow.Api.Entities
{
	public class TaskItem
	{
		public Guid Id { get; set; }

		public string TenantId { get; set; } = string.Empty;

		public string Title { get; set; } = string.Empty;

		public string? Description { get; set; }

		public bool IsComplete { get; set; }

		public DateTimeOffset CreatedAt { get; set; }

		public ICollection<TaskAttachment> Attachments { get; set; } = [];
	}
}
