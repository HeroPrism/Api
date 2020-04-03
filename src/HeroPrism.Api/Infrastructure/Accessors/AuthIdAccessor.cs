using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HeroPrism.Api.Infrastructure.Accessors
{
    public class AuthIdAccessor : IAuthIdAccessor
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public AuthIdAccessor(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public async Task<string> GetAuthId(CancellationToken cancellationToken)
        {
            // HACK: GET AROUND ASYNC COMPLAINT
            await Task.CompletedTask;

            var user = _contextAccessor.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
#if DEBUG
                return "auth0|5e86867df23bc20bf0c5b5fb";
#else
                    return null;
#endif
            }

            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}