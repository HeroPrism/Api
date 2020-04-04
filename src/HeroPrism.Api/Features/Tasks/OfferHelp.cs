using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut;
using Cosmonaut.Extensions;
using HeroPrism.Api.Infrastructure;
using HeroPrism.Api.Infrastructure.Exceptions;
using HeroPrism.Data;
using MediatR;
using Microsoft.Azure.Documents.Linq;
using StreamChat;

namespace HeroPrism.Api.Features.Tasks
{
    public class OfferHelpRequest : IRequest<OfferHelpResponse>
    {
        public string TaskId { get; set; }
    }

    public class OfferHelpRequestHandler : IRequestHandler<OfferHelpRequest, OfferHelpResponse>
    {
        private readonly ICosmosStore<HelpOffered> _helpStore;
        private readonly ICosmosStore<HelpTask> _taskStore;
        private readonly HeroPrismSession _session;
        private readonly IClient _chatClient;

        public OfferHelpRequestHandler(ICosmosStore<HelpOffered> helpStore, ICosmosStore<HelpTask> taskStore,
            HeroPrismSession session, IClient chatClient)
        {
            _helpStore = helpStore;
            _taskStore = taskStore;
            _session = session;
            _chatClient = chatClient;
        }

        public async Task<OfferHelpResponse> Handle(OfferHelpRequest request, CancellationToken cancellationToken)
        {
            // Check to make sure they aren't already helping
            var offered = await _helpStore.Query()
                .Where(c => c.HelperId == _session.UserId)
                .Where(c => c.TaskId == request.TaskId)
                .FirstOrDefaultAsync(cancellationToken);

            if (offered == null)
            {
                var task = await _taskStore.FindAsync(request.TaskId, cancellationToken: cancellationToken);

                if (task == null)
                {
                    throw new EntityNotFoundException();
                }

                offered = new HelpOffered()
                {
                    Id = Guid.NewGuid().ToString(),
                    RequesterId = task.UserId,
                    HelperId = _session.UserId,
                    TaskId = task.Id,
                };

                await CreateChatRoom(offered.Id, offered.RequesterId, offered.HelperId, cancellationToken);

                await _helpStore.AddAsync(offered, cancellationToken: cancellationToken);
            }

            return new OfferHelpResponse() {ChatId = offered.Id};
        }

        private async Task CreateChatRoom(string id, string requesterId, string helperId,
            CancellationToken cancellationToken)
        {
            var channel = _chatClient.Channel("messaging", id);
            
            await channel.Create(id);
            await channel.AddMembers(new[] {requesterId, helperId});
        }
    }

    public class OfferHelpResponse
    {
        public string ChatId { get; set; }
    }
}