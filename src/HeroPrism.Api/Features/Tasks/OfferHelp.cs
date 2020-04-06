using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut;
using Cosmonaut.Extensions;
using FluentValidation;
using HeroPrism.Api.Infrastructure;
using HeroPrism.Api.Infrastructure.Exceptions;
using HeroPrism.Data;
using MediatR;
using Nerdino.Controllerless;
using StreamChat;

namespace HeroPrism.Api.Features.Tasks
{
    [ApiRequest("tasks", "help", ActionType.Create, false)]
    public class OfferHelpRequest : IRequest<OfferHelpResponse>
    {
        public string TaskId { get; set; }
    }

    public class OfferHelpRequestValidator : AbstractValidator<OfferHelpRequest>
    {
        public OfferHelpRequestValidator()
        {
            RuleFor(c => c.TaskId).NotEmpty();
        }
    }

    public class OfferHelpResponse
    {
        public string ChatId { get; set; }
    }

    public class OfferHelpRequestHandler : IRequestHandler<OfferHelpRequest, OfferHelpResponse>
    {
        private readonly ICosmosStore<Offer> _offerStore;
        private readonly ICosmosStore<HelpTask> _taskStore;
        private readonly HeroPrismSession _session;
        private readonly IClient _chatClient;

        public OfferHelpRequestHandler(ICosmosStore<Offer> offerStore, ICosmosStore<HelpTask> taskStore,
            HeroPrismSession session, IClient chatClient)
        {
            _offerStore = offerStore;
            _taskStore = taskStore;
            _session = session;
            _chatClient = chatClient;
        }

        public async Task<OfferHelpResponse> Handle(OfferHelpRequest request, CancellationToken cancellationToken)
        {
            // Check to make sure they aren't already helping
            var offer = await _offerStore.Query()
                .Where(c => c.HelperId == _session.UserId)
                .Where(c => c.TaskId == request.TaskId)
                .FirstOrDefaultAsync(cancellationToken);

            if (offer == null)
            {
                var task = await _taskStore.FindAsync(request.TaskId, cancellationToken: cancellationToken);

                if (task == null || !task.IsOpen())
                {
                    throw new EntityNotFoundException();
                }

                if (task.UserId == _session.UserId)
                {
                    // You can't offer to help your own task.
                    throw new UnauthorizedAccessException();
                }

                offer = new Offer()
                {
                    Id = Guid.NewGuid().ToString(),
                    HelperId = _session.UserId,
                    TaskId = task.Id,
                };

                await CreateChatRoom(offer.Id, task.UserId, offer.HelperId, cancellationToken);

                await _offerStore.AddAsync(offer, cancellationToken: cancellationToken);

                task.Status = TaskStatuses.Active;

                await _taskStore.UpsertAsync(task, cancellationToken: cancellationToken);
            }

            return new OfferHelpResponse() {ChatId = offer.Id};
        }

        private async Task CreateChatRoom(string id, string requesterId, string helperId,
            CancellationToken cancellationToken)
        {
            var channel = _chatClient.Channel("messaging", id);

            await channel.Create(requesterId, new[] {requesterId, helperId});
        }
    }
}