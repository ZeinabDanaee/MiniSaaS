public class CurrentUserService
{
    private readonly IHttpContextAccessor _http;

    public CurrentUserService(IHttpContextAccessor http)
    {
        _http = http;
    }

    public Guid TenantId
    {
        get
        {
            var httpContext = _http.HttpContext;

            if (httpContext == null)
                return Guid.Empty;

            var claim = httpContext.User.FindFirst("tenant_id");

            if (claim == null)
                return Guid.Empty;

            return Guid.Parse(claim.Value);
        }
    }

    public Guid UserId
    {
        get
        {
            var httpContext = _http.HttpContext;

            var claim = httpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

            return claim != null ? Guid.Parse(claim.Value) : Guid.Empty;
        }
    }
}