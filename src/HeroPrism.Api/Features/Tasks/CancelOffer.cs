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
using StreamChat;

namespace HeroPrism.Api.Features.Tasks
{
    [ApiRequest("tasks", "offers/cancel", ActionType.Create, false)]
    public class CancelOfferRequest : IRequest
    {
        public string TaskId { get; set; }
    }

    public class CancelOfferRequestHandler : IRequestHandler<CancelOfferRequest>
    {
        private readonly ICosmosStore<HelpTask> _taskStore;
        private readonly ICosmosStore<Offer> _offerStore;
        private readonly IClient _chatClient;
        private readonly HeroPrismSession _session;

        public CancelOfferRequestHandler(ICosmosStore<HelpTask> taskStore, ICosmosStore<Offer> offerStore,
            IClient chatClient, HeroPrismSession session)
        {
            _taskStore = taskStore;
            _offerStore = offerStore;
            _chatClient = chatClient;
            _session = session;
        }

        public async Task<Unit> Handle(CancelOfferRequest request, CancellationToken cancellationToken)
        {
            var task = await _taskStore.FindAsync(request.TaskId, cancellationToken: cancellationToken);

            if (task == null || !task.IsOpen())
            {
                throw new EntityNotFoundException();
            }

            var offer = await _offerStore.Query()
                .Where(c => c.HelperId == _session.UserId)
                .Where(c => c.TaskId == request.TaskId)
                .FirstOrDefaultAsync(cancellationToken);

            // They've not offered help so just return, nothing to delete. 
            if (offer == null)
            {
                return Unit.Value;
            }

            await DeleteChatroom(offer.Id);

            await _offerStore.RemoveAsync(offer, cancellationToken: cancellationToken);

            return Unit.Value;
        }

        private async Task DeleteChatroom(string offerId)
        {
            await _chatClient.Channel("messaging", offerId).Delete();
        }
    }
}