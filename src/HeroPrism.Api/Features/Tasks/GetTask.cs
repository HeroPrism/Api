using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut;
using Cosmonaut.Extensions;
using HeroPrism.Api.Infrastructure;
using HeroPrism.Api.Infrastructure.Exceptions;
using HeroPrism.Data;
using MediatR;
using Nerdino.Controllerless;

namespace HeroPrism.Api.Features.Tasks
{
    [ApiRequest("tasks", "{taskId}", ActionType.Read, false)]
    public class GetTaskRequest : IRequest<GetTaskRequestResponse>
    {
        public string TaskId { get; set; }
    }

    public class GetTaskRequestResponse
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreateDateTime { get; set; }
        public TaskStatuses Status { get; set; }
        public TaskCategory Category { get; set; }
        public IEnumerable<ChatUserResponse> Offers { get; set; }
    }

    public class GetTaskRequestHandler : IRequestHandler<GetTaskRequest, GetTaskRequestResponse>
    {
        private readonly ICosmosStore<HelpTask> _taskStore;
        private readonly ICosmosStore<HelpOffered> _offerStore;
        private readonly ICosmosStore<User> _userStore;
        private readonly HeroPrismSession _session;

        public GetTaskRequestHandler(ICosmosStore<HelpTask> taskStore, ICosmosStore<HelpOffered> offerStore,
            ICosmosStore<User> userStore, HeroPrismSession session)
        {
            _taskStore = taskStore;
            _offerStore = offerStore;
            _userStore = userStore;
            _session = session;
        }

        public async Task<GetTaskRequestResponse> Handle(GetTaskRequest request, CancellationToken cancellationToken)
        {
            var task = await _taskStore.FindAsync(request.TaskId, cancellationToken: cancellationToken);

            if (task == null)
            {
                throw new EntityNotFoundException();
            }

            if (task.UserId != _session.UserId)
            {
                throw new UnauthorizedAccessException();
            }

            var response = new GetTaskRequestResponse()
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Category = task.Category,
                Status = task.Status,
                CreateDateTime = task.CreatedDateTime,
                Offers = await GetOffers(request.TaskId, cancellationToken)
            };

            return response;
        }

        private async Task<IEnumerable<ChatUserResponse>> GetOffers(string taskId, CancellationToken cancellationToken)
        {
            var offers = await _offerStore.Query()
                .Where(o => o.TaskId == taskId)
                .ToListAsync(cancellationToken);

            var userIds = offers.Select(c => c.HelperId).Distinct();

            var users = await _userStore.Query().Where(c => userIds.Contains(c.Id)).ToListAsync(cancellationToken);

            var userLookup = users.ToDictionary(c => c.Id);

            var offerResponses = new List<ChatUserResponse>();
            foreach (var offer in offers)
            {

                var user = userLookup[offer.HelperId];

                if (user == null)
                {
                    continue;
                }

                var response = new ChatUserResponse()
                {
                    UserId = offer.HelperId,
                    Score = user.Score,
                    ChatId = offer.Id,
                    PictureId = user.PictureId,
                    FirstName = user.FirstName
                };

                offerResponses.Add(response);
            }

            return offerResponses;
        }
    }
}