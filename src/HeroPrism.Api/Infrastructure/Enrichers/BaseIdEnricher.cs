using Serilog.Core;
using Serilog.Events;

namespace HeroPrism.Api.Infrastructure.Enrichers
{
    public abstract class BaseIdEnricher : ILogEventEnricher
    {
        protected abstract string IdName { get; }
        
        protected abstract string GetId();
        
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var property = propertyFactory.CreateProperty(IdName, GetId());
            logEvent.AddPropertyIfAbsent(property);
        }
    }
}