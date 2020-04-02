using System.Threading;
using System.Threading.Tasks;
using Cosmonaut;
using HeroPrism.Api.Infrastructure;
using HeroPrism.Data;
using MediatR;
using Nerdino.Controllerless;

namespace HeroPrism.Api.Features.Users
{
    [ApiRequest("users", "", ActionType.Read, false)]
    public class GetUserRequest : IRequest<GetUserResponse>
    {
        
    }

    public class GetUserRequestHandler : IRequestHandler<GetUserRequest, GetUserResponse>
    {
        private readonly HeroPrismSession _session;

        public GetUserRequestHandler(HeroPrismSession session)
        {
            _session = session;
        }


        public Task<GetUserResponse> Handle(GetUserRequest request, CancellationToken cancellationToken)
        {
            var response = new GetUserResponse()
            {
                PictureId = _session.User.PictureId,
                FirstName = _session.User.FirstName,
                LastName = _session.User.LastName,
                Score = _session.User.Score,
                UserType = _session.User.UserType
            };

            return Task.FromResult(response);
        }
    }

    public class GetUserResponse
    {
        public int PictureId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public UserTypes UserType { get; set; }
        public int Score { get; set; }
    }
}
