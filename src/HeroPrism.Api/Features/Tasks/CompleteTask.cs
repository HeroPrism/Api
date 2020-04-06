using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut;
using Cosmonaut.Extensions;
using FluentValidation;
using HeroPrism.Api.Features.Chat;
using HeroPrism.Api.Infrastructure;
using HeroPrism.Api.Infrastructure.Exceptions;
using HeroPrism.Data;
using MediatR;
using Nerdino.Controllerless;

namespace HeroPrism.Api.Features.Tasks
{
    [ApiRequest("tasks", "complete", ActionType.Create, false)]
    public class CompleteTaskRequest : IRequest
    {
        public string ChatId { get; set; }
    }

    public class CompleteTaskRequestHandler : IRequestHandler<CompleteTaskRequest>
    {
        private readonly ICosmosStore<HelpTask> _taskStore;
        private readonly ICosmosStore<Offer> _offerStore;
        private readonly ICosmosStore<User> _userStore;
        private readonly HeroPrismSession _session;
        private readonly IMediator _mediator;

        public CompleteTaskRequestHandler(ICosmosStore<HelpTask> taskStore,
            ICosmosStore<Offer> offerStore,
            ICosmosStore<User> userStore,
            HeroPrismSession session,
            IMediator mediator)
        {
            _taskStore = taskStore;
            _offerStore = offerStore;
            _userStore = userStore;
            _session = session;
            _mediator = mediator;
        }

        public async Task<Unit> Handle(CompleteTaskRequest request, CancellationToken cancellationToken)
        {
            var offer = await _offerStore.Query()
                .FirstOrDefaultAsync(c => c.Id == request.ChatId, cancellationToken);

            if (offer == null)
            {
                // Trying to complete something they don't have access to. 
                throw new EntityNotFoundException();
            }

            var task = await _taskStore.FindAsync(offer.TaskId, cancellationToken: cancellationToken);

            if (task == null)
            {
                throw new EntityNotFoundException();
            }

            if (task.UserId != _session.UserId)
            {
                throw new UnauthorizedAccessException();
            }

            // Assign Score
            await AssignScore(offer, cancellationToken);

            // Mark as completed
            await MarkTaskAsCompleted(task, offer.HelperId, cancellationToken);

            // Remove all chat rooms associated to task
            await RemoveChatRooms(task.Id, cancellationToken);

            return Unit.Value;
        }

        private async Task RemoveChatRooms(string taskId, CancellationToken cancellationToken)
        {
            await _mediator.Send(new RemoveChatroomsCommand() {TaskId = taskId}, cancellationToken);
        }

        private async Task MarkTaskAsCompleted(HelpTask task, string helperId, CancellationToken cancellationToken)
        {
            task.Status = TaskStatuses.Completed;
            task.HelperId = helperId;
            await _taskStore.UpdateAsync(task, cancellationToken: cancellationToken);
        }

        private async Task AssignScore(Offer offer, CancellationToken cancellationToken)
        {
            var user = await _userStore.Query()
                .FirstAsync(c => c.Id == offer.HelperId, cancellationToken);

            user.Score += 1;

            await _userStore.UpdateAsync(user, cancellationToken: cancellationToken);
        }
    }

    public class CompleteTaskRequestValidator : AbstractValidator<CompleteTaskRequest>
    {
        public CompleteTaskRequestValidator()
        {
            RuleFor(c => c.ChatId).NotEmpty().NotNull();
        }
    }
}