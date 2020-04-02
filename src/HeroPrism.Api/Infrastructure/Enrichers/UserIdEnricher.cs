using System.Threading;
using HeroPrism.Api.Infrastructure.Accessors;

namespace HeroPrism.Api.Infrastructure.Enrichers
{
    public class UserIdEnricher : BaseIdEnricher
    {
        private readonly IUserIdAccessor _userIdAccessor;

        public UserIdEnricher(IUserIdAccessor userIdAccessor)
        {
            _userIdAccessor = userIdAccessor;
        }

        protected override string IdName => "UserId";

        protected override string GetId()
        {
            return _userIdAccessor.GetUserId(CancellationToken.None).Result;
        }
    }
}