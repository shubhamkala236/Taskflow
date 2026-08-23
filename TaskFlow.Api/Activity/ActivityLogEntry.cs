namespace TaskFlow.Api.Activity
{
	public class ActivityLogEntry
	{
		public string Id { get; set; } = Guid.NewGuid().ToString();

		public string TenantId { get; set; } = string.Empty;

		public string TaskItemId { get; set; } = string.Empty;

		public string Actor { get; set; } = "anonymous";

		public string Action { get; set; } = string.Empty;

		public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

		public string? Details { get; set; }
	}
}
