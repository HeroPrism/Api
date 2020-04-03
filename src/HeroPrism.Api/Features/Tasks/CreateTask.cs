using System;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut;
using FluentValidation;
using HeroPrism.Api.Infrastructure;
using HeroPrism.Data;
using MediatR;
using Microsoft.Azure.Documents.Spatial;
using Nerdino.Controllerless;

namespace HeroPrism.Api.Features.Tasks
{
    // POST - /tasks
    [ApiRequest("tasks", "", ActionType.Create, false)]
    public class CreateTaskRequest : IRequest<CreateTaskResponse>
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string ZipCode { get; set; }
        
        public TaskCategory? Category { get; set; }
    }

    public class CreateTaskRequestHandler : IRequestHandler<CreateTaskRequest, CreateTaskResponse>
    {
        private readonly HeroPrismSession _session;
        private readonly ICosmosStore<HelpTask> _taskStore;

        public CreateTaskRequestHandler(HeroPrismSession session, ICosmosStore<HelpTask> taskStore)
        {
            _session = session;
            _taskStore = taskStore;
        }

        public async Task<CreateTaskResponse> Handle(CreateTaskRequest request, CancellationToken cancellationToken)
        {
            var helpTask = new HelpTask()
            {
                Id = Guid.NewGuid().ToString(),
                Title = request.Title,
                Description = request.Description,
                ZipCode = request.ZipCode,
                ZipLocation = CreatePointFromZip(request.ZipCode),
                UserId = _session.UserId,
                Category = request.Category.GetValueOrDefault(TaskCategory.Item)
            };

            var addResponse = await _taskStore.AddAsync(helpTask, cancellationToken: cancellationToken);

            if (!addResponse.IsSuccess)
            {
                // TODO: DO SOMETHING?!@#?!
            }

            return new CreateTaskResponse() {Id = helpTask.Id};
        }

        private static Point CreatePointFromZip(string zipCode)
        {
            // TODO: MAKE THIS ACTUALLY WORK
            var random = new Random();
            var afterDecimalLat = random.NextDouble();
            var afterDecimalLong = random.NextDouble();

            return new Point(new Position(-111 - afterDecimalLong, 33 + afterDecimalLat));
        }
    }

    public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
    {
        public CreateTaskRequestValidator()
        {
            RuleFor(c => c.Title).NotEmpty();
            RuleFor(c => c.Description).NotEmpty();
            RuleFor(c => c.ZipCode).NotEmpty();
        }
    }

    public class CreateTaskResponse
    {
        public string Id { get; set; }
    }
}