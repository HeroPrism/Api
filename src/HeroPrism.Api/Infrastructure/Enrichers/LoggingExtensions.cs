using System;
using CorrelationId;
using HeroPrism.Api.Infrastructure.Accessors;
using Serilog;
using Serilog.Configuration;

namespace HeroPrism.Api.Infrastructure.Enrichers
{
    public static class LoggingExtensions
    {
        public static LoggerConfiguration WithAuthId(this LoggerEnrichmentConfiguration enrichmentConfiguration, IAuthIdAccessor authIdAccessor)
        {
            if (enrichmentConfiguration == null) 
                throw new ArgumentNullException(nameof(enrichmentConfiguration));

            var userIdEnricher = new UserIdEnricher(authIdAccessor);

            return enrichmentConfiguration.With(userIdEnricher);
        }

        public static LoggerConfiguration WithCorrelationId(this LoggerEnrichmentConfiguration enrichmentConfiguration, ICorrelationContextAccessor correlationAccessor)
        {
            if (enrichmentConfiguration == null) 
                throw new ArgumentNullException(nameof(enrichmentConfiguration));

            var correlationIdEnricher = new CorrelationIdEnricher(correlationAccessor);

            return enrichmentConfiguration.With(correlationIdEnricher);
        }
    }
}