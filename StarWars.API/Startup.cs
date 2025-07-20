using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using StarWars.Api.Authorization;
using StarWars.Api.Services;
using StarWars.Api.Settings;
using StarWars.Interface;
using StarWars.Model.ViewModels;
using StarWars.Repository;
using System;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace StarWars.Api
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
            services.AddCors(options => {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                    .WithOrigins(Configuration["CorsUrl"].Split(';'))
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            var issuer = $"https://{Configuration["Auth0:Domain"]}/";
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => {
                    options.Authority = issuer;
                    options.Audience = Configuration["Auth0:Audience"];
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = ClaimTypes.NameIdentifier
                    };
                });
            services.AddAuthorization(options => {
                options.AddPolicy("read:messages", policy => policy.Requirements.Add(new HasScopeRequirement("read:messages", issuer)));
            });
            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();

            Mapper.Initialize(cfg => {
                cfg.AddProfile<AutoMapperProfile>();
            });

            var webJetSettings = Configuration.GetSection("WebJetSettings");
            var movieCodeSettings = Configuration.GetSection("MovieCodeSettings");

            // Configurations
            services.Configure<WebJetSettings>(webJetSettings);
            services.Configure<MovieCodeSettings>(movieCodeSettings);
            services.AddMemoryCache();

            // Use SQL Server DB
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration["ConnectionStrings:DefaultConnection"],
                    b => b.MigrationsAssembly("StarWars.API")));

            // Use InMemoryDatabase
            //services.AddDbContext<ApplicationDbContext>(o => {
            //    o.UseInMemoryDatabase("Movies");
            //});

            services.AddSingleton(provider => webJetSettings.Get<WebJetSettings>());
            services.AddSingleton(provider => movieCodeSettings.Get<MovieCodeSettings>());
            services.AddTransient<IMoviesRepository, MovieRepository>();
            services.AddScoped<IMovieService, MovieService>();
            services.AddScoped<IWebJetService, WebJetService>();
            services.AddHttpClient("WebJetClient", client => {
                client.BaseAddress = new Uri(webJetSettings["BaseUrl"]);
                client.DefaultRequestHeaders.Add("x-access-token", webJetSettings["AccessToken"]);
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(5)) // Set lifetime to five minutes
            .AddPolicyHandler(GetRetryPolicy()); // Set retry policy

            services
                .AddRouting(o => o.LowercaseUrls = true)
                .AddControllers()
                .AddJsonOptions(o => o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

            services.AddFluentValidationAutoValidation();
            services.AddFluentValidationClientsideAdapters();
            services.AddValidatorsFromAssemblyContaining<MovieViewValidator>(ServiceLifetime.Transient);

            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "StarWars API", Version = "v1" });
            });

            // DB seeding with fake items
            services.AddTransient<IDatabaseInitializer, DatabaseInitializer>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("CorsPolicy");
            app.UseHttpsRedirection();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.DocumentTitle = "Swagger - StarWars API";
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "StarWars API V1");
                c.RoutePrefix = string.Empty;
            });
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        static AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}
