using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HeroPrism.Api.Infrastructure.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILoggerFactory _loggerFactory;

        public LoggingBehavior(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var requestName = request.GetType().FullName;
            
            var logger = _loggerFactory.CreateLogger(requestName);
            
            logger.LogInformation($"Starting...");
            
            try
            {
                var response = await next();
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error...");
                throw;
            }
            finally
            {
                logger.LogInformation($"Completed...");    
            }
        }
    }
}