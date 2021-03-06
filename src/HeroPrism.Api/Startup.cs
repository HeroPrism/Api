using System;
using System.Security.Claims;
using System.Threading;
using CorrelationId;
using Cosmonaut;
using Cosmonaut.Extensions.Microsoft.DependencyInjection;
using FluentValidation.AspNetCore;
using HeroPrism.Api.Features.Tasks;
using HeroPrism.Api.Infrastructure;
using HeroPrism.Api.Infrastructure.Accessors;
using HeroPrism.Api.Infrastructure.Behaviors;
using HeroPrism.Api.Infrastructure.Extensions;
using HeroPrism.Api.Infrastructure.Settings;
using HeroPrism.Data;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Nerdino.Controllerless;
using Newtonsoft.Json.Converters;

namespace HeroPrism.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddMediatRWithPipelines();

            services.AddHttpClient("AzureMaps", (serviceProvider, client) =>
            {
                var settings = serviceProvider.GetService<AzureMapSettings>();

                client.BaseAddress = new Uri(settings.BaseUrl);
            });
            services.AddAzureMap(Configuration.GetSection("AzureMaps"));
            services.AddCosmosDb(Configuration.GetSection("CosmosDb"));
            services.AddSearch(Configuration.GetSection("Search"));

            services.AddHeroPrismSession();

            services.AddAuth0(Configuration.GetSection("Auth0"));

            services.AddChat(Configuration.GetSection("Chat"));

            services.AddControllerless<ApiRequest>();
            
            services.AddApplicationInsightsTelemetry(options =>
            {
                options.InstrumentationKey = Configuration.GetValue<string>("ApplicationInsights:InstructionKey");
                options.DeveloperMode = Environment.IsDevelopment();
            });

            services.AddSwaggerDocument();
            
            services.AddMvcCore(options =>
            {
                options.EnableEndpointRouting = false;
                options.Filters.Add(new HeroPrismExceptionFilter());
            })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
            })
            .AddApiExplorer()
            .AddAuthorization()
            .AddCors()
            .AddFluentValidation(options => { options.RegisterValidatorsFromAssemblyContaining<CreateTaskRequestValidator>(); });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseCorrelationId(new CorrelationIdOptions
            {
                Header = "X-HeroPrism-CorrelationId",
                UseGuidForCorrelationId = true,
                UpdateTraceIdentifier = true
            });
            
            app.UseRouting();

            app.UseAuthentication();
            
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            
            app.UseCors(builder => builder.WithOrigins(Configuration.GetValue("CORS", "").Split(",")).AllowAnyMethod().AllowAnyHeader());
            
            app.UseOpenApi();
            app.UseSwaggerUi3();
        }
    }
}