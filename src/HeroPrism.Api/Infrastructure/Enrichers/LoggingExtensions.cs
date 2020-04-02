using System;
using CorrelationId;
using HeroPrism.Api.Infrastructure.Accessors;
using Serilog;
using Serilog.Configuration;

namespace HeroPrism.Api.Infrastructure.Enrichers
{
    public static class LoggingExtensions
    {
        public static LoggerConfiguration WithUserId(this LoggerEnrichmentConfiguration enrichmentConfiguration, IUserIdAccessor userIdAccessor)
        {
            if (enrichmentConfiguration == null) 
                throw new ArgumentNullException(nameof(enrichmentConfiguration));

            var userIdEnricher = new UserIdEnricher(userIdAccessor);

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