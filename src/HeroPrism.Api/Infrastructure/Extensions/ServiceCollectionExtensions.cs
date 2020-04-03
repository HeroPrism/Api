using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using CorrelationId;
using Cosmonaut;
using Cosmonaut.Extensions.Microsoft.DependencyInjection;
using HeroPrism.Api.Infrastructure.Accessors;
using HeroPrism.Api.Infrastructure.Behaviors;
using HeroPrism.Api.Infrastructure.Settings;
using HeroPrism.Data;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HeroPrism.Api.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCosmosDb(this IServiceCollection services,
            IConfigurationSection section)
        {
            var dbName = section.GetValue<string>("DatabaseName");
            var authKey = section.GetValue<string>("AuthKey");
            var url = section.GetValue<string>("Url");

            var cosmosSettings = new CosmosStoreSettings(dbName, url, authKey);
            
            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> {new StringEnumConverter()}
            };

            cosmosSettings.JsonSerializerSettings = settings;

            services.AddCosmosStore<HelpTask>(cosmosSettings);
            services.AddCosmosStore<User>(cosmosSettings);

            return services;
        }

        public static IServiceCollection AddSearch(this IServiceCollection services,
            IConfigurationSection section)
        {
            var searchSettings = new SearchSettings();
            section.Bind(searchSettings);
            services.AddSingleton(searchSettings);

            return services;
        }

        public static IServiceCollection AddMediatRWithPipelines(this IServiceCollection services)
        {
            services.AddMediatR(typeof(Startup));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RegistrationBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            
            return services;
        }

        public static IServiceCollection AddAuth0(this IServiceCollection services, IConfigurationSection section)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Authority = section.GetValue<string>("Authority");;
                options.Audience = section.GetValue<string>("Audience");
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    NameClaimType = ClaimTypes.NameIdentifier
                };
            });

            return services;
        }

        public static IServiceCollection AddHeroPrismSession(this IServiceCollection services)
        {
            services.AddCorrelationId();
            services.AddTransient<IAuthIdAccessor, AuthIdAccessor>();
            services.AddTransient<IHeroPrismSessionAccessor, HeroPrismSessionAccessor>();
            services.AddScoped(context => context.GetService<IHeroPrismSessionAccessor>().Get(CancellationToken.None).Result);

            return services;
        }
    }
}