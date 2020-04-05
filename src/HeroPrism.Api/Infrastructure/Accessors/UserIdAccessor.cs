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
                return "test";
#else
                    return null;
#endif
            }

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return userId;
            }

            return userId.Replace("|", "_");
        }
    }
}