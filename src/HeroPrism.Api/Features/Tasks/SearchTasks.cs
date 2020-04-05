using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut;
using Cosmonaut.Extensions;
using FluentValidation;
using HeroPrism.Api.Infrastructure;
using HeroPrism.Api.Infrastructure.Settings;
using HeroPrism.Data;
using MediatR;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Nerdino.Controllerless;

namespace HeroPrism.Api.Features.Tasks
{
    // Tasks/search
    [ApiRequest("tasks", "search", ActionType.Create, true)]
    public class SearchTasksRequest : IRequest<SearchResponse>
    {
        public BoundsRequest Bounds { get; set; }

        public string ToSearchString()
        {
            var boundsArray = new[]
            {
                Bounds.NW,
                Bounds.SW,
                Bounds.SE,
                Bounds.NE,
                Bounds.NW,
            };
            var builder = new StringBuilder();
            builder.Append("geo.intersects(ZipLocation, geography'POLYGON((");
            builder.Append(string.Join(", ", boundsArray.Select(b => b.ToSearchString())));
            builder.Append("))')");

            return builder.ToString();
        }
    }
    
    public class SearchTaskRequestValidator : AbstractValidator<SearchTasksRequest>
    {
        public SearchTaskRequestValidator()
        {
            RuleFor(c => c.Bounds).SetValidator(new BoundsRequestValidator()).NotNull();
        }
    }
    
    public class SearchResponse
    {
        public IEnumerable<TaskResponse> Tasks { get; set; }
    }

    public class TaskResponse
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ZipCode { get; set; }
        public CoordinateDto Coordinate { get; set; }
        public DateTime CreateDateTime { get; set; }
        public TaskStatuses Status { get; set; }
        public TaskCategory Category { get; set; }
        public PublicUserResponse Requester { get; set; }
    }

    public class PublicUserResponse
    {
        public string FirstName { get; set; }

        public int Score { get; set; }

        public UserTypes UserType { get; set; }

        public int PictureId { get; set; }
    }

    public class CoordinateDto
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public string ToSearchString()
        {
            return $"{Longitude} {Latitude}";
        }
    }
    

    public class BoundsRequest
    {
        public CoordinateDto SW { get; set; }
        public CoordinateDto NW { get; set; }
        public CoordinateDto SE { get; set; }
        public CoordinateDto NE { get; set; }
    }
    
    public class BoundsRequestValidator : AbstractValidator<BoundsRequest>
    {
        public BoundsRequestValidator()
        {
            RuleFor(c => c.NE).NotNull();
            RuleFor(c => c.NW).NotNull();
            RuleFor(c => c.SE).NotNull();
            RuleFor(c => c.SW).NotNull();
        }
    }

    public class SearchTasksRequestHandler : IRequestHandler<SearchTasksRequest, SearchResponse>
    {
        private readonly HeroPrismSession _session;
        private readonly SearchSettings _searchSettings;
        private readonly ICosmosStore<User> _userStore;

        public SearchTasksRequestHandler(HeroPrismSession session, SearchSettings searchSettings,
            ICosmosStore<User> userStore)
        {
            _session = session;
            _searchSettings = searchSettings;
            _userStore = userStore;
        }

        public async Task<SearchResponse> Handle(SearchTasksRequest request, CancellationToken cancellationToken)
        {
            var endpoint = _searchSettings.EndpointName;
            var credential = new SearchCredentials(_searchSettings.ApiKey);

            var searchClient = new SearchIndexClient(endpoint, _searchSettings.IndexName, credential);

            var parameters = new SearchParameters
            {
                Filter = $"Status ne '{TaskStatuses.Completed}' and Status ne '{TaskStatuses.Deleted}' and " + request.ToSearchString(),
            };

            searchClient.UseHttpGetForQueries = true;

            var searchResponse = await searchClient.Documents.SearchAsync<HelpTask>("*", parameters,
                cancellationToken: cancellationToken);

            var response = new SearchResponse();

            if (searchResponse.Count == 0)
            {
                return response;
            }

            var userIds = searchResponse.Results.Select(c => c.Document.UserId).Distinct();

            var users = await _userStore.Query().Where(c => userIds.Contains(c.Id)).ToListAsync(cancellationToken);

            var userLookup = users.ToDictionary(c => c.Id);
            
            var responseTasks = new List<TaskResponse>();
                
            foreach (var searchResult in searchResponse.Results)
            {
                var taskResponse = CreateTaskResponse(searchResult.Document);

                if (!userLookup.ContainsKey(searchResult.Document.UserId))
                {
                    userLookup[searchResult.Document.UserId] = await _userStore.FindAsync(searchResult.Document.UserId,
                        cancellationToken: cancellationToken);
                }

                var user = userLookup[searchResult.Document.UserId];

                if (user == null)
                {
                    continue;
                }

                taskResponse.Requester = new PublicUserResponse()
                {
                    FirstName = user.FirstName,
                    Score = user.Score,
                    PictureId = user.PictureId,
                    UserType = user.UserType
                };
                
                responseTasks.Add(taskResponse);
            }

            response.Tasks = responseTasks;

            return response;
        }

        private static TaskResponse CreateTaskResponse(HelpTask helpTask)
        {
            
            // TODO: AUTOMAPPER
            var responseTask = new TaskResponse()
            {
                Id = helpTask.Id,
                Coordinate = new CoordinateDto()
                {
                    Latitude = helpTask.ZipLocation.Position.Latitude,
                    Longitude = helpTask.ZipLocation.Position.Longitude
                },
                Description = helpTask.Description,
                Title = helpTask.Title,
                ZipCode = helpTask.ZipCode,
                CreateDateTime = helpTask.CreatedDateTime,
                Status = helpTask.Status,
                Category = helpTask.Category,
            };

            return responseTask;
        }
    }
}