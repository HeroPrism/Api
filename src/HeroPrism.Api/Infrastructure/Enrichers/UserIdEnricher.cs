using System.Threading;
using HeroPrism.Api.Infrastructure.Accessors;

namespace HeroPrism.Api.Infrastructure.Enrichers
{
    public class UserIdEnricher : BaseIdEnricher
    {
        private readonly HeroPrismSession _session;

        public UserIdEnricher(HeroPrismSession session)
        {
            _session = session;
        }

        protected override string IdName => "UserId";

        protected override string GetId()
        {
            return _session.UserId;
        }
    }
}