using System.Threading;
using System.Threading.Tasks;
using Cosmonaut;
using FluentValidation;
using HeroPrism.Api.Infrastructure;
using HeroPrism.Data;
using MediatR;
using Nerdino.Controllerless;

namespace HeroPrism.Api.Features.Users
{
    [ApiRequest("users", "", ActionType.Update, false)]
    public class UpdateUserRequest : IRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public UserTypes? UserType { get; set; }
    }
    
    public class UpdateUserRequestHandler : IRequestHandler<UpdateUserRequest, Unit>
    {
        private readonly ICosmosStore<User> _userStore;
        private readonly HeroPrismSession _session;

        public UpdateUserRequestHandler(ICosmosStore<User> userStore, HeroPrismSession session)
        {
            _userStore = userStore;
            _session = session;
        }
        
        public async Task<Unit> Handle(UpdateUserRequest request, CancellationToken cancellationToken)
        {
            var user = _session.User;

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.UserType = request.UserType.GetValueOrDefault(UserTypes.Individual);

            await _userStore.UpsertAsync(user, cancellationToken: cancellationToken);
            
            return Unit.Value;
        }
    }

    public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserRequestValidator()
        {
            RuleFor(c => c.FirstName).NotEmpty();
            RuleFor(c => c.LastName).NotEmpty();
            RuleFor(c => c.UserType).NotEmpty();
        }
    }
}