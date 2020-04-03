using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut;
using Cosmonaut.Extensions;
using HeroPrism.Api.Infrastructure.Exceptions;
using HeroPrism.Data;
using MediatR;

namespace HeroPrism.Api.Infrastructure.Behaviors
{
    public class RegistrationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ICosmosStore<User> _userStore;
        private readonly HeroPrismSession _session;

        public RegistrationBehavior(ICosmosStore<User> userStore, HeroPrismSession session)
        {
            _userStore = userStore;
            _session = session;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            // Prior to this point the auth should have been checked if needed.  
            // Don't want to check registration on the registration API
            if (request is IDoNotCheckRegistration || string.IsNullOrWhiteSpace(_session.AuthId))
            {
                return await next();
            }

            var user = await _userStore.Query()
                .FirstOrDefaultAsync(c => c.AuthId == _session.AuthId, cancellationToken);

            if (user == null)
            {
                throw new NoRegistrationException();
            }

            _session.UserId = user.Id;
            _session.User = user;

            return await next();
        }
    }
}