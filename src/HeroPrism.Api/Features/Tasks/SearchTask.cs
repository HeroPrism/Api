using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using HeroPrism.Api.Infrastructure;
using HeroPrism.Api.Infrastructure.Settings;
using HeroPrism.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Nerdino.Controllerless;
using TaskStatus = HeroPrism.Data.TaskStatus;

namespace HeroPrism.Api.Tasks
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

    public class BoundsRequest
    {
        public CoordinateDto SW { get; set; }
        public CoordinateDto NW { get; set; }
        public CoordinateDto SE { get; set; }
        public CoordinateDto NE { get; set; }
    }

    public class SearchTasksRequestHandler : IRequestHandler<SearchTasksRequest, SearchResponse>
    {
        private readonly HeroPrismSession _session;
        private readonly SearchSettings _searchSettings;

        public SearchTasksRequestHandler(HeroPrismSession session, SearchSettings searchSettings)
        {
            _session = session;
            _searchSettings = searchSettings;
        }

        public async Task<SearchResponse> Handle(SearchTasksRequest request, CancellationToken cancellationToken)
        {
            var endpoint = _searchSettings.EndpointName;
            var credential = new SearchCredentials(_searchSettings.ApiKey);

            var searchClient = new SearchIndexClient(endpoint, _searchSettings.IndexName, credential);
            
            var parameters = new SearchParameters
            {
                Filter = $"Status ne '{TaskStatus.Completed}' and " + request.ToSearchString(),
            };

            searchClient.UseHttpGetForQueries = true;
            
            var searchResponse = await searchClient.Documents.SearchAsync<HelpTask>("*", parameters,
                cancellationToken: cancellationToken);

            var response = new SearchResponse();

            if (searchResponse.Count == 0)
            {
                return response;
            }

            response.Tasks = searchResponse.Results.Select(c => new TaskResponse()
            {
                Id = c.Document.Id, 
                Coordinate = new CoordinateDto() 
                { 
                    Latitude = c.Document.ZipLocation.Position.Latitude,
                    Longitude = c.Document.ZipLocation.Position.Longitude
                },
                Description = c.Document.Description,
                Title = c.Document.Title,
                ZipCode = c.Document.ZipCode,
                CreateDateTime = c.Document.CreatedDateTime,
                Status = c.Document.Status,
                Category = c.Document.Category
            });

            return response;
        }
    }

    public class SearchTaskRequestValidator : AbstractValidator<SearchTasksRequest>
    {
        public SearchTaskRequestValidator()
        {
            RuleFor(c => c.Bounds).SetValidator(new BoundsRequestValidator()).NotNull();
        }
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
        public TaskStatus Status { get; set; }
        
        public TaskCategory Category { get; set; }
        
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

    public class UserResponse
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public int Score { get; set; }
        public string PictureUrl { get; set; }
    }
}