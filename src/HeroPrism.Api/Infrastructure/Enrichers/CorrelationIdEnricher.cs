using CorrelationId;

namespace HeroPrism.Api.Infrastructure.Enrichers
{
    public class CorrelationIdEnricher : BaseIdEnricher
    {
        private readonly ICorrelationContextAccessor _correlationAccessor;

        public CorrelationIdEnricher(ICorrelationContextAccessor correlationAccessor)
        {
            _correlationAccessor = correlationAccessor;
        }

        protected override string IdName => "CorrelationId";

        protected override string GetId()
        {
            return _correlationAccessor.CorrelationContext?.CorrelationId;
        }
    }
}