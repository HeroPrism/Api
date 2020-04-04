using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut;
using Cosmonaut.Extensions;
using HeroPrism.Api.Infrastructure;
using HeroPrism.Data;
using MediatR;
using Nerdino.Controllerless;

namespace HeroPrism.Api.Features.Tasks
{
    [ApiRequest("tasks", "offers", ActionType.Read, false)]
    public class GetMyOffersRequest : IRequest<GetMyOffersResponse>
    {
    }

    public class GetMyOffersRequestHandler : IRequestHandler<GetMyOffersRequest, GetMyOffersResponse>
    {
        private readonly ICosmosStore<HelpOffered> _offeredStore;
        private readonly ICosmosStore<HelpTask> _taskStore;
        private readonly HeroPrismSession _session;

        public GetMyOffersRequestHandler(ICosmosStore<HelpOffered> offeredStore, ICosmosStore<HelpTask> taskStore, HeroPrismSession session)
        {
            _offeredStore = offeredStore;
            _taskStore = taskStore;
            _session = session;
        }
        
        public async Task<GetMyOffersResponse> Handle(GetMyOffersRequest request, CancellationToken cancellationToken)
        {
            var offers = await _offeredStore.Query()
                .Where(e => e.HelperId == _session.UserId)
                .ToListAsync(cancellationToken);

            var response = new GetMyOffersResponse();
            if (!offers.Any())
            {
                return response;
            }

            var taskIds = offers.Select(o => o.TaskId).Distinct().ToList();

            var tasks = await _taskStore.Query()
                .Where(t => taskIds.Contains(t.Id))
                .ToListAsync(cancellationToken);

            if (!tasks.Any())
            {
                return response;
            }

            response.Tasks = tasks?.Select(c => new MyTaskResponse()
            {
                Id = c.Id,
                Coordinate = new CoordinateDto()
                {
                    Latitude = c.ZipLocation.Position.Latitude,
                    Longitude = c.ZipLocation.Position.Longitude
                },
                Description = c.Description,
                Title = c.Title,
                ZipCode = c.ZipCode,
                CreateDateTime = c.CreatedDateTime,
                Status = c.Status,
                Category = c.Category,
            });

            return response;
        }
    }

    public class GetMyOffersResponse
    {
        public IEnumerable<MyTaskResponse> Tasks { get; set; }
    }
}