using TaskFlow.Api.Activity;

namespace TaskFlow.Api.Services
{
	public interface IActivityLogService
	{
		Task EnsureDatabaseAndContainerExistsAsync(CancellationToken cancellationToken = default);

		Task LogAsync(ActivityLogEntry entry, CancellationToken cancellationToken = default);
	}
}
