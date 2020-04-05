using System;
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
    [ApiRequest("tasks", "requests", ActionType.Read, false)]
    public class GetMyRequestsRequest : IRequest<GetMyRequestsResponse>
    {
    }
    
    public class GetMyRequestsResponse
    {
        public IEnumerable<MyRequestTaskResponse> Tasks { get; set; }
    }
    
    public class MyRequestTaskResponse
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ZipCode { get; set; }
        public CoordinateDto Coordinate { get; set; }
        public DateTime CreateDateTime { get; set; }
        public TaskStatuses Status { get; set; }
        public TaskCategory Category { get; set; }
    }

    public class GetMyRequestsRequestHandler : IRequestHandler<GetMyRequestsRequest, GetMyRequestsResponse>
    {
        private readonly ICosmosStore<HelpTask> _taskStore;
        private readonly HeroPrismSession _session;

        public GetMyRequestsRequestHandler(ICosmosStore<HelpTask> taskStore, HeroPrismSession session)
        {
            _taskStore = taskStore;
            _session = session;
        }

        public async Task<GetMyRequestsResponse> Handle(GetMyRequestsRequest request, CancellationToken cancellationToken)
        {
            var tasks = await _taskStore.Query()
                .Where(e => e.UserId == _session.UserId)
                .Where(e=>e.Status != TaskStatuses.Deleted)
                .ToListAsync(cancellationToken);
            

            // TODO: AUTOMAPPER
            var response = new GetMyRequestsResponse()
            {
                Tasks = tasks?.Select(c => new MyRequestTaskResponse()
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
                })
            };

            return response;
        }
    }
}