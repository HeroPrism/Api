using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut;
using Cosmonaut.Extensions;
using FluentValidation;
using HeroPrism.Api.Infrastructure;
using HeroPrism.Data;
using MediatR;
using Nerdino.Controllerless;
using StreamChat;

namespace HeroPrism.Api.Features.Tasks
{
    [ApiRequest("tasks", "delete", ActionType.Create, false)]
    public class RemoveTaskRequest : IRequest
    {
        public string TaskId { get; set; }
    }
    
    public class RemoveTaskRequestValidator : AbstractValidator<OfferHelpRequest>
    {
        public RemoveTaskRequestValidator()
        {
            RuleFor(c => c.TaskId).NotEmpty().NotNull();
        }
    }

    public class RemoveTaskRequestHandler : IRequestHandler<RemoveTaskRequest>
    {
        private readonly ICosmosStore<HelpTask> _taskStore;
        private readonly ICosmosStore<Offer> _offeredStore;
        private readonly HeroPrismSession _session;
        private readonly IClient _chatClient;

        public RemoveTaskRequestHandler(ICosmosStore<HelpTask> taskStore, ICosmosStore<Offer> offeredStore, HeroPrismSession session, IClient chatClient)
        {
            _taskStore = taskStore;
            _offeredStore = offeredStore;
            _session = session;
            _chatClient = chatClient;
        }

        public async Task<Unit> Handle(RemoveTaskRequest request, CancellationToken cancellationToken)
        {
            var task = await _taskStore.FindAsync(request.TaskId, cancellationToken: cancellationToken);

            if (task.UserId != _session.UserId)
            {
                throw new UnauthorizedAccessException();
            }

            //await RemoveChatRooms(task.Id);
            task.Status = TaskStatuses.Deleted;

            await _taskStore.UpdateAsync(task, cancellationToken: cancellationToken);

            return Unit.Value;
        }

        private async Task RemoveChatRooms(string taskId)
        {
            var helpOffered = await _offeredStore.Query()
                .Where(c => c.TaskId == taskId).ToListAsync();

            foreach (var help in helpOffered)
            {
                await _chatClient.Channel(help.Id).Delete();
                await _offeredStore.RemoveAsync(help);
            }
        }
    }
}