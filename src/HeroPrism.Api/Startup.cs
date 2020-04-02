using System.Security.Claims;
using System.Threading;
using CorrelationId;
using Cosmonaut;
using Cosmonaut.Extensions.Microsoft.DependencyInjection;
using FluentValidation.AspNetCore;
using HeroPrism.Api.Infrastructure;
using HeroPrism.Api.Infrastructure.Accessors;
using HeroPrism.Api.Infrastructure.Behaviors;
using HeroPrism.Api.Infrastructure.Extensions;
using HeroPrism.Api.Infrastructure.Settings;
using HeroPrism.Api.Tasks;
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
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddMediatRWithPipelines();
            services.AddCosmosDb(Configuration.GetSection("CosmosDb"));
            services.AddSearch(Configuration.GetSection("Search"));

            services.AddHeroPrismSession();

            services.AddAuth0(Configuration.GetSection("Auth0"));

            services.AddControllerless<ApiRequest>();
            
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
        }
    }
}