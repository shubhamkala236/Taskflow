using Microsoft.Identity.Web;

namespace TaskFlow.Api.Tenancy
{
	public sealed class TenantProvider(IHttpContextAccessor accessor) : ITenantProvider
	{
		public string TenantId =>
			accessor.HttpContext?.User.GetTenantId()
			?? throw new InvalidOperationException("No tenant id claim present on the authenticated user.");
	}
}
