namespace MiniSaaS.Auth.Services
{
    using System.Security.Claims;
   
    public class CurrentUserService
    {
        private readonly IHttpContextAccessor _context;

        public CurrentUserService(IHttpContextAccessor context)
        {
            _context = context;
        }

        public Guid? TenantId
        {
            get
            {
                var value = _context.HttpContext?
                    .User?
                    .FindFirst("tenant_id")?
                    .Value;

                return value != null ? Guid.Parse(value) : null;
            }
        }

        public Guid? UserId
        {
            get
            {
                var value = _context.HttpContext?
                    .User?
                    .FindFirst(ClaimTypes.NameIdentifier)?
                    .Value;

                return value != null ? Guid.Parse(value) : null;
            }
        }
    }
}
