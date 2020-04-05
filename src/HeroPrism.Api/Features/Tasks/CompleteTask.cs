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

namespace HeroPrism.Api.Features.Tasks
{
    [ApiRequest("tasks", "completed", ActionType.Create, false)]
    public class CompleteTaskRequest : IRequest
    {
        public string TaskId { get; set; }
    }

    public class CompleteTaskRequestHandler : IRequestHandler<CompleteTaskRequest>
    {
        private readonly ICosmosStore<HelpTask> _taskStore;
        private readonly ICosmosStore<HelpOffered> _offeredStore;
        private readonly ICosmosStore<User> _userStore;
        private readonly HeroPrismSession _session;

        public CompleteTaskRequestHandler(ICosmosStore<HelpTask> taskStore,
            ICosmosStore<HelpOffered> offeredStore,
            ICosmosStore<User> userStore,
            HeroPrismSession session)
        {
            _taskStore = taskStore;
            _offeredStore = offeredStore;
            _userStore = userStore;
            _session = session;
        }

        public async Task<Unit> Handle(CompleteTaskRequest request, CancellationToken cancellationToken)
        {
            var task = await _taskStore.FindAsync(request.TaskId, cancellationToken: cancellationToken);

            if (task == null)
            {
                throw new EntityNotFoundException();
            }

            var offeredHelp = await _offeredStore.Query()
                .Where(c => c.HelperId == _session.UserId || c.RequesterId == _session.UserId)
                .FirstOrDefaultAsync(c => c.TaskId == request.TaskId, cancellationToken);

            if (offeredHelp == null)
            {
                // Trying to complete something they don't have access to. 
                throw new UnauthorizedAccessException();
            }

            if (offeredHelp.HelperId == _session.UserId)
            {
                offeredHelp.HelperCompleted = true;
            }

            if (offeredHelp.RequesterId == _session.UserId)
            {
                offeredHelp.RequesterCompleted = true;
            }

            if (offeredHelp.HelperCompleted && offeredHelp.RequesterCompleted)
            {
                // Assign Score
                await AssignScore(offeredHelp, cancellationToken);

                // Mark as completed
                await MarkTaskAsCompleted(task, cancellationToken);
                
                // Remove all chat rooms associated to task
                await RemoveChatRooms(task, cancellationToken);
            }

            await _offeredStore.UpdateAsync(offeredHelp, cancellationToken: cancellationToken);
            
            return Unit.Value;
        }

        private async Task RemoveChatRooms(HelpTask task, CancellationToken cancellationToken)
        {
            // TODO: Figure out how to delete chatrooms. 
            await Task.CompletedTask;
        }

        private async Task MarkTaskAsCompleted(HelpTask task, CancellationToken cancellationToken)
        {
            task.Status = TaskStatuses.Completed;
            await _taskStore.UpdateAsync(task, cancellationToken: cancellationToken);
        }

        private async Task AssignScore(HelpOffered offeredHelp, CancellationToken cancellationToken)
        {
            var user = await _userStore.Query()
                .FirstAsync(c => c.Id == offeredHelp.HelperId, cancellationToken);

            user.Score += 5;

            await _userStore.UpdateAsync(user, cancellationToken: cancellationToken);
        }
    }

    public class CompleteTaskRequestValidator : AbstractValidator<CompleteTaskRequest>
    {
        public CompleteTaskRequestValidator()
        {
            RuleFor(c => c.TaskId).NotEmpty().NotNull();
        }
    }
}