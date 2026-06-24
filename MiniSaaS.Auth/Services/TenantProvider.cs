namespace MiniSaaS.Auth.Services
{
  
    public interface ITenantProvider
    {
        Guid TenantId { get; }
    }

    public class TenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _http;

        public TenantProvider(IHttpContextAccessor http)
        {
            _http = http;
        }

        public Guid TenantId
        {
            get
            {
                var claim = _http.HttpContext?
                    .User
                    .FindFirst("tenant_id");

                if (claim == null)
                    return Guid.Empty;

                return Guid.Parse(claim.Value);
            }
        }
    }
}
