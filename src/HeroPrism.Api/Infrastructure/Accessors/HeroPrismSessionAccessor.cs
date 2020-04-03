using System.Threading;
using System.Threading.Tasks;
using CorrelationId;

namespace HeroPrism.Api.Infrastructure.Accessors
{
    public class HeroPrismSessionAccessor : IHeroPrismSessionAccessor
    {
        private readonly IAuthIdAccessor _authIdAccessor;
        private readonly ICorrelationContextAccessor _correlationAccessor;

        public HeroPrismSessionAccessor(IAuthIdAccessor authIdAccessor, ICorrelationContextAccessor correlationAccessor)
        {
            _authIdAccessor = authIdAccessor;
            _correlationAccessor = correlationAccessor;
        }

        public async Task<HeroPrismSession> Get(CancellationToken cancellationToken)
        {
            var correlationId = _correlationAccessor.CorrelationContext.CorrelationId;
            var userId = await _authIdAccessor.GetAuthId(cancellationToken);

            return new HeroPrismSession(correlationId, userId);
        }
    }
}