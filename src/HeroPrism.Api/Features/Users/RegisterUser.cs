using System.Threading;
using System.Threading.Tasks;
using Cosmonaut;
using FluentValidation;
using HeroPrism.Api.Infrastructure;
using HeroPrism.Data;
using MediatR;
using Nerdino.Controllerless;
using StreamChat;
using User = HeroPrism.Data.User;

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

    public class RegisterUserRequestHandler : IRequestHandler<RegisterUserRequest>
    {
        private readonly ICosmosStore<User> _userStore;
        private readonly HeroPrismSession _session;
        private readonly IClient _chatClient;

        public RegisterUserRequestHandler(ICosmosStore<User> userStore, HeroPrismSession session, IClient chatClient)
        {
            _userStore = userStore;
            _session = session;
            _chatClient = chatClient;
        }

        public async Task<Unit> Handle(RegisterUserRequest request, CancellationToken cancellationToken)
        {
            var user = _session.User ?? new User() {Id = _session.UserId};

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.UserType = request.UserType;
            user.Score = 1;
            user.PictureId = request.PictureId; 

            await _userStore.UpsertAsync(user, cancellationToken: cancellationToken);
         
            await CreateChatUser(user.Id, cancellationToken);

            return Unit.Value;
        }

        private async Task CreateChatUser(string userId, CancellationToken cancellationToken)
        {
            var user = new StreamChat.User
            {
                ID = userId,
                Role = Role.User
            };

            await _chatClient.Users.Update(user);
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