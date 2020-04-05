using System;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut;
using HeroPrism.Api.Features.Tasks;
using HeroPrism.Api.Infrastructure;
using HeroPrism.Api.Infrastructure.Exceptions;
using HeroPrism.Data;
using MediatR;
using Nerdino.Controllerless;

namespace HeroPrism.Api.Features.Chat
{
    [ApiRequest("chat", "{roomId}", ActionType.Read, false)]
    public class GetChatRoomRequest : IRequest<GetChatRoomRequestResponse>
    {
        public string RoomId { get; set; }
    }

    public class GetChatRoomRequestResponse
    {
        public string RoomId { get; set; }
        public ChatTaskResponse Task { get; set; }
        public OtherUserResponse Other { get; set; }
    }

    public class ChatTaskResponse
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreateDateTime { get; set; }
        public TaskStatuses Status { get; set; }
        public TaskCategory Category { get; set; }
    }

    public class OtherUserResponse
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public int Score { get; set; }
        public int PictureId { get; set; }
        public bool IsRequester { get; set; }
    }

    public class GetChatRoomRequestHandler : IRequestHandler<GetChatRoomRequest, GetChatRoomRequestResponse>
    {
        private readonly ICosmosStore<Offer> _offerStore;
        private readonly ICosmosStore<User> _userStore;
        private readonly ICosmosStore<HelpTask> _taskStore;
        private readonly HeroPrismSession _session;

        public GetChatRoomRequestHandler(ICosmosStore<Offer> offerStore, 
            ICosmosStore<User> userStore,
            ICosmosStore<HelpTask> taskStore,
            HeroPrismSession session)
        {
            _offerStore = offerStore;
            _userStore = userStore;
            _taskStore = taskStore;
            _session = session;
        }

        public async Task<GetChatRoomRequestResponse> Handle(GetChatRoomRequest request,
            CancellationToken cancellationToken)
        {
            var offer = await _offerStore.FindAsync(request.RoomId, cancellationToken: cancellationToken);

            if (offer == null)
            {
                throw new EntityNotFoundException();
            }

            if (offer.HelperId != _session.UserId && offer.RequesterId != _session.UserId)
            {
                throw new UnauthorizedAccessException();
            }

            var task = await _taskStore.FindAsync(offer.TaskId, cancellationToken: cancellationToken);

            if (task == null || !task.IsOpen())
            {
                throw new EntityNotFoundException();
            }

            var isCurrentUserRequester = offer.RequesterId == _session.UserId;

            var userId = isCurrentUserRequester ? offer.HelperId : offer.RequesterId;

            var user = await _userStore.FindAsync(userId, cancellationToken: cancellationToken);

            if (user == null)
            {
                throw new EntityNotFoundException();
            }

            var response = new GetChatRoomRequestResponse()
            {
                RoomId = request.RoomId,
                Task = new ChatTaskResponse()
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    Category = task.Category,
                    Status = task.Status,
                    CreateDateTime = task.CreatedDateTime
                },
                Other = new OtherUserResponse()
                {
                    FirstName = user.FirstName,
                    Score = user.Score,
                    PictureId = user.PictureId,
                    Id = user.Id,
                    IsRequester = !isCurrentUserRequester
                }
            };

            return response;
        }
    }
}