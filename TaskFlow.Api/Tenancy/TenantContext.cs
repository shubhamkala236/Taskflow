namespace TaskFlow.Api.Tenancy
{
	/// <summary>
	/// No real auth/multi-tenancy exists yet, so every request resolves to this single tenant.
	/// This means every Cosmos activity-log write currently lands on one physical partition —
	/// a fine illustration of why partition key choice matters, since it only pays off once
	/// real per-tenant values exist. Replace with per-request tenant resolution once auth lands.
	/// </summary>
	public static class TenantContext
	{
		public const string CurrentTenantId = "default-tenant";
	}
}
