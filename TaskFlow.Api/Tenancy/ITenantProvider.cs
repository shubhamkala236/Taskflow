namespace TaskFlow.Api.Tenancy
{
	public interface ITenantProvider
	{
		string TenantId { get; }
	}
}
