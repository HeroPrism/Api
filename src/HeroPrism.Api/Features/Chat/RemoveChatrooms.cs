using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut;
using Cosmonaut.Extensions;
using HeroPrism.Data;
using MediatR;
using StreamChat;

namespace HeroPrism.Api.Features.Chat
{
    public class RemoveChatroomsCommand : IRequest
    {
        public string TaskId { get; set; }
    }

    public class RemoveChatroomsCommandHandler : IRequestHandler<RemoveChatroomsCommand>
    {
        private readonly ICosmosStore<Offer> _offerStore;
        private readonly IClient _chatClient;

        public RemoveChatroomsCommandHandler(ICosmosStore<Offer> offerStore, IClient chatClient)
        {
            _offerStore = offerStore;
            _chatClient = chatClient;
        }
        
        public async Task<Unit> Handle(RemoveChatroomsCommand request, CancellationToken cancellationToken)
        {
            var offers = await _offerStore.Query()
                .Where(c => c.TaskId == request.TaskId)
                .ToListAsync(cancellationToken);

            foreach (var offer in offers)
            {
                await _chatClient.Channel("messaging", offer.Id).Delete();
            }
            
            return Unit.Value;
        }
    }
}