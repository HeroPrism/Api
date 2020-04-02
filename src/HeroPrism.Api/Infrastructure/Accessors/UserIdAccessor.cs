using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HeroPrism.Api.Infrastructure.Accessors
{
    public class UserIdAccessor : IUserIdAccessor
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public UserIdAccessor(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public async Task<string> GetUserId(CancellationToken cancellationToken)
        {
            // HACK: GET AROUND ASYNC COMPLAINT
            await Task.CompletedTask;

            var user = _contextAccessor.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
#if DEBUG
                return "craig";
#else
                    return null;
#endif
            }

            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}