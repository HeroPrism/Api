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
    [ApiRequest("tasks", "offers", ActionType.Read, false)]
    public class GetMyOffersRequest : IRequest<GetMyOffersResponse>
    {
    }
    
    public class GetMyOffersResponse
    {
        public IEnumerable<MyOfferTaskResponse> Tasks { get; set; }
    }

    public class MyOfferTaskResponse
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ZipCode { get; set; }
        public CoordinateDto Coordinate { get; set; }
        public DateTime CreateDateTime { get; set; }
        public TaskStatuses Status { get; set; }
        public TaskCategory Category { get; set; }
        public ChatUserResponse Requester { get; set; }
    }

    public class ChatUserResponse
    {
        public string ChatId { get; set; }
        public string UserId { get; set; }
        public int Score { get; set; }
        public int PictureId { get; set; }
        public string FirstName { get; set; }
    }

    public class GetMyOffersRequestHandler : IRequestHandler<GetMyOffersRequest, GetMyOffersResponse>
    {
        private readonly ICosmosStore<HelpOffered> _offeredStore;
        private readonly ICosmosStore<HelpTask> _taskStore;
        private readonly ICosmosStore<User> _userStore;
        private readonly HeroPrismSession _session;

        public GetMyOffersRequestHandler(ICosmosStore<HelpOffered> offeredStore, ICosmosStore<HelpTask> taskStore, ICosmosStore<User> userStore, HeroPrismSession session)
        {
            _offeredStore = offeredStore;
            _taskStore = taskStore;
            _userStore = userStore;
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

            var taskIds = offers.Select(o => o.TaskId).Distinct().ToHashSet();

            var tasks = await _taskStore.Query()
                .Where(t => taskIds.Contains(t.Id))
                .ToListAsync(cancellationToken);

            if (!tasks.Any())
            {
                return response;
            }

            var userIds = tasks.Select(c => c.UserId);
            
            var users = await _userStore.Query().Where(c => userIds.Contains(c.Id)).ToListAsync(cancellationToken);

            var userLookup = users.ToDictionary(c => c.Id);

            response.Tasks = MapToMyOfferTaskResponses(tasks, offers, userLookup);;
            
            return response;
        }

        private static IEnumerable<MyOfferTaskResponse> MapToMyOfferTaskResponses(List<HelpTask> tasks, IReadOnlyCollection<HelpOffered> offers, IReadOnlyDictionary<string, User> userLookup)
        {
            foreach (var task in tasks)
            {
                var offer = offers.FirstOrDefault(c => c.TaskId == task.Id);

                if (offer == null)
                {
                    continue;
                }

                var taskResponse = new MyOfferTaskResponse()
                {
                    Id = task.Id,
                    Coordinate = new CoordinateDto()
                    {
                        Latitude = task.ZipLocation.Position.Latitude,
                        Longitude = task.ZipLocation.Position.Longitude
                    },
                    Description = task.Description,
                    Title = task.Title,
                    ZipCode = task.ZipCode,
                    CreateDateTime = task.CreatedDateTime,
                    Status = task.Status,
                    Category = task.Category,
                };

                var user = userLookup[task.UserId];

                if (user == null)
                {
                    continue;
                }

                taskResponse.Requester = new ChatUserResponse()
                {
                    UserId = user.Id,
                    ChatId = offer.Id,
                    Score = user.Score,
                    PictureId = user.PictureId,
                    FirstName = user.FirstName
                };

                yield return taskResponse;
            }
        }
    }

   
}