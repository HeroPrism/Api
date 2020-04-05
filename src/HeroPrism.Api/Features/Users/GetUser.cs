using System;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut;
using HeroPrism.Api.Infrastructure;
using HeroPrism.Data;
using MediatR;
using Nerdino.Controllerless;
using StreamChat;

namespace HeroPrism.Api.Features.Users
{
    [ApiRequest("users", "", ActionType.Read, false)]
    public class GetUserRequest : IRequest<GetUserResponse>
    {
    }
    
    public class GetUserResponse
    {
        public int PictureId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public UserTypes UserType { get; set; }
        public int Score { get; set; }

        public ChatTokenResponse ChatToken { get; set; }
    }

    public class ChatTokenResponse
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }

    public class GetUserRequestHandler : IRequestHandler<GetUserRequest, GetUserResponse>
    {
        private readonly HeroPrismSession _session;
        private readonly IClient _chatClient;

        public GetUserRequestHandler(HeroPrismSession session, IClient chatClient)
        {
            _session = session;
            _chatClient = chatClient;
        }

        public Task<GetUserResponse> Handle(GetUserRequest request, CancellationToken cancellationToken)
        {
            var expiration = DateTime.UtcNow.AddHours(5);

            var response = new GetUserResponse()
            {
                PictureId = _session.User.PictureId,
                FirstName = _session.User.FirstName,
                LastName = _session.User.LastName,
                Score = _session.User.Score,
                UserType = _session.User.UserType,
                ChatToken = GetChatToken(_session.UserId)
            };

            return Task.FromResult(response);
        }

        private ChatTokenResponse GetChatToken(string userId)
        {
            var expiration = DateTime.UtcNow.AddHours(5);
       
            var chatToken = _chatClient.CreateUserToken(userId, expiration);

            var token = new ChatTokenResponse()
            {
                Token = chatToken,
                Expiration = expiration
            };

            return token;
        }
    }

   
}