using System.Threading;
using HeroPrism.Api.Infrastructure.Accessors;

namespace HeroPrism.Api.Infrastructure.Enrichers
{
    public class UserIdEnricher : BaseIdEnricher
    {
        private readonly IAuthIdAccessor _authIdAccessor;

        public UserIdEnricher(IAuthIdAccessor authIdAccessor)
        {
            _authIdAccessor = authIdAccessor;
        }

        protected override string IdName => "UserId";

        protected override string GetId()
        {
            return _authIdAccessor.GetAuthId(CancellationToken.None).Result;
        }
    }
}