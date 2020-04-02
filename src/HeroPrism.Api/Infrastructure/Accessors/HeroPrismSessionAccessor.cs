using System.Threading;
using System.Threading.Tasks;
using CorrelationId;

namespace HeroPrism.Api.Infrastructure.Accessors
{
    public class HeroPrismSessionAccessor : IHeroPrismSessionAccessor
    {
        private readonly IUserIdAccessor _userIdAccessor;
        private readonly ICorrelationContextAccessor _correlationAccessor;

        public HeroPrismSessionAccessor(IUserIdAccessor userIdAccessor, ICorrelationContextAccessor correlationAccessor)
        {
            _userIdAccessor = userIdAccessor;
            _correlationAccessor = correlationAccessor;
        }

        public async Task<HeroPrismSession> Get(CancellationToken cancellationToken)
        {
            var correlationId = _correlationAccessor.CorrelationContext.CorrelationId;
            var userId = await _userIdAccessor.GetUserId(cancellationToken);

            if (userId == null)
            {
                // TODO : FIGURE OUT WHAT TO DO HERE.  THIS SHOULDN'T HAPPEN
            }

            return new HeroPrismSession(correlationId, userId);
        }
    }
}