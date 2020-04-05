using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut;
using Cosmonaut.Extensions;
using FluentValidation;
using HeroPrism.Api.Features.Chat;
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
        private readonly ICosmosStore<Offer> _offerStore;
        private readonly HeroPrismSession _session;
        private readonly IMediator _mediator;

        public RemoveTaskRequestHandler(ICosmosStore<HelpTask> taskStore, ICosmosStore<Offer> offerStore,
            HeroPrismSession session, IMediator mediator)
        {
            _taskStore = taskStore;
            _offerStore = offerStore;
            _session = session;
            _mediator = mediator;
        }

        public async Task<Unit> Handle(RemoveTaskRequest request, CancellationToken cancellationToken)
        {
            var task = await _taskStore.FindAsync(request.TaskId, cancellationToken: cancellationToken);

            if (task.UserId != _session.UserId)
            {
                throw new UnauthorizedAccessException();
            }

            if (task.Status == TaskStatuses.Deleted)
            {
                return Unit.Value;
            }

            await RemoveChatRooms(task.Id, cancellationToken);

            task.Status = TaskStatuses.Deleted;

            await _taskStore.UpdateAsync(task, cancellationToken: cancellationToken);

            return Unit.Value;
        }

        private async Task RemoveChatRooms(string taskId, CancellationToken cancellationToken)
        {
            await _mediator.Send(new RemoveChatroomsCommand() {TaskId = taskId}, cancellationToken);
        }
    }
}