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
    [ApiRequest("users", "", ActionType.Create, false)]
    public class RegisterUserRequest : IRequest, IDoNotCheckRegistration
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public UserTypes UserType { get; set; } = UserTypes.Individual;
        public int PictureId { get; set; } = 1;
    }

    public class RegisterUserRequestHandler : IRequestHandler<RegisterUserRequest, Unit>
    {
        private readonly ICosmosStore<User> _userStore;
        private readonly HeroPrismSession _session;

        public RegisterUserRequestHandler(ICosmosStore<User> userStore, HeroPrismSession session)
        {
            _userStore = userStore;
            _session = session;
        }

        public async Task<Unit> Handle(RegisterUserRequest request, CancellationToken cancellationToken)
        {
            var user = _session.User ?? new User() {AuthId = _session.AuthId};

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.UserType = request.UserType;
            user.Score = 1;
            user.PictureId = request.PictureId; 

            await _userStore.UpsertAsync(user, cancellationToken: cancellationToken);

            return Unit.Value;
        }
    }

    public class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
    {
        public RegisterUserRequestValidator()
        {
            RuleFor(c => c.FirstName).NotEmpty();
            RuleFor(c => c.LastName).NotEmpty();
        }
    }
}