using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut;
using FluentValidation;
using HeroPrism.Api.Infrastructure;
using HeroPrism.Data;
using MediatR;
using Microsoft.AspNetCore.WebUtilities;
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
    
    public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
    {
        public CreateTaskRequestValidator()
        {
            RuleFor(c => c.Title).NotEmpty().NotNull();
            RuleFor(c => c.Description).NotEmpty().NotNull();
            RuleFor(c => c.ZipCode).NotEmpty().NotNull();
        }
    }
    
    public class CreateTaskResponse
    {
        public string Id { get; set; }
    }

    public class CreateTaskRequestHandler : IRequestHandler<CreateTaskRequest, CreateTaskResponse>
    {
        private static Random _random = new Random();
        private readonly HeroPrismSession _session;
        private readonly ICosmosStore<HelpTask> _taskStore;
        private readonly HttpClient _httpClient;
        private readonly AzureMapSettings _mapSettings;

        public CreateTaskRequestHandler(HeroPrismSession session, ICosmosStore<HelpTask> taskStore, IHttpClientFactory clientFactory, AzureMapSettings mapSettings)
        {
            _session = session;
            _taskStore = taskStore;
            _httpClient = clientFactory.CreateClient("AzureMaps");
            _mapSettings = mapSettings;
        }

        public async Task<CreateTaskResponse> Handle(CreateTaskRequest request, CancellationToken cancellationToken)
        {
            var helpTask = new HelpTask()
            {
                Id = Guid.NewGuid().ToString(),
                Title = request.Title,
                Description = request.Description,
                ZipCode = request.ZipCode,
                ZipLocation = await CreatePointFromZip(request.ZipCode),
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

        private async Task<Point> CreatePointFromZip(string zipCode)
        {
            var parameters = new Dictionary<string, string>
            {
                {"api-version", _mapSettings.Version},
                {"query", $"{zipCode}"},
                {"subscription-key", _mapSettings.SubscriptionKey},
                {"countrySet", "US"},
                {"extendedPostalCodesFor", "Geo"},
                {"limit", "1"}
            };

            var query = QueryHelpers.AddQueryString("/search/address/json", parameters);
            var response = await _httpClient.GetAsync(query);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.ReasonPhrase);
            }

            var result = await response.Content.ReadAsAsync<AzureMapResponse>();

            if (result.results == null || !result.results.Any())
            {
                throw new Exception($"Unable to get geocoordinates for zipcode {zipCode}");
            }

            var firstResult = result.results[0];

            var randomLongitude = GetRandomBetween(firstResult.boundingBox.topLeftPoint.lon,
                firstResult.boundingBox.btmRightPoint.lon);
            var randomLatitude = GetRandomBetween(firstResult.boundingBox.btmRightPoint.lat,
                firstResult.boundingBox.topLeftPoint.lat);
            
            return new Point(new Position(randomLongitude, randomLatitude));
        }

        private double GetRandomBetween(double min, double max)
        {
            return _random.NextDouble() * (min - max) + min;
        }

        public class Coordinate
        {
            public double lat { get; set; }
            public double lon { get; set; }
        }

        public class BoundingBox
        {
            public Coordinate topLeftPoint { get; set; }
            public Coordinate btmRightPoint { get; set; }
        }

        public class SearchResult
        {
            public Coordinate position { get; set; }
            public BoundingBox boundingBox { get; set; }
        }

        public class AzureMapResponse
        {
            public List<SearchResult> results { get; set; }
        }
    }

    public class AzureMapSettings
    {
        public string BaseUrl { get; set; }
        public string Version { get; set; }
        public string SubscriptionKey { get; set; }
        
    }
}
