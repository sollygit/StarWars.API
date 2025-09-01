using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
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
using StarWars.API.Middleware;
using StarWars.API.Services;
using StarWars.Model.ViewModels;
using StarWars.Repository;
using System;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarWars.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var origins = builder.Configuration["CorsUrl"]!.Split(';');

            // Add services to the container.
            builder.Services.AddCors(options => {
                options.AddPolicy("CorsPolicy", builder => builder
                    .WithOrigins(origins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            var issuer = $"https://{builder.Configuration["Auth0:Domain"]}/";
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => {
                    options.Authority = issuer;
                    options.Audience = builder.Configuration["Auth0:Audience"];
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = ClaimTypes.NameIdentifier
                    };
                });
            builder.Services.AddAuthorization(options => {
                options.AddPolicy("read:messages", policy => policy.Requirements.Add(new HasScopeRequirement("read:messages", issuer)));
            });
            builder.Services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();

            // Auto Mapper Configurations
            Mapper.Initialize(cfg => { cfg.AddProfile<AutoMapperProfile>(); });

            // Add memory cache
            builder.Services.AddMemoryCache();

            // Use SQL Server DB
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration["ConnectionStrings:DefaultConnection"],
                    b => b.MigrationsAssembly("StarWars.API")));

            var webJetSettings = builder.Configuration.GetSection("WebJetSettings");

            builder.Services.Configure<WebJetSettings>(webJetSettings);
            builder.Services.AddSingleton<IOrderService, OrderService>();
            builder.Services.AddTransient<IDatabaseInitializer, DatabaseInitializer>();
            builder.Services.AddTransient<IMoviesRepository, MovieRepository>();
            builder.Services.AddScoped<IMovieService, MovieService>();
            builder.Services.AddScoped<IWebJetService, WebJetService>();
            builder.Services
                .AddHttpClient("WebJetClient", client => {
                    client.BaseAddress = new Uri(webJetSettings["BaseUrl"]!);
                    client.DefaultRequestHeaders.Add("x-access-token", webJetSettings["AccessToken"]);
                })
                .SetHandlerLifetime(TimeSpan.FromMinutes(5)) // Set lifetime to five minutes
                .AddPolicyHandler(GetRetryPolicy()); // Set retry policy

            builder.Services
                .AddRouting(o => o.LowercaseUrls = true)
                .AddControllers()
                .AddJsonOptions(options => {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });

            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddFluentValidationClientsideAdapters();
            builder.Services.AddValidatorsFromAssemblyContaining<MovieViewValidator>(ServiceLifetime.Transient);
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "StarWars API", Version = "v1" });
            });

            var app = builder.Build();

            app.UseMiddleware<CorrelationIdMiddleware>();

            // Load and cache orders at startup
            using (var scope = app.Services.CreateScope())
            {
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
                await orderService.CacheOrdersAsync(env.ContentRootPath);
            }

            // DB seeding from JSON file for soft startup
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
                var dbInitializer = services.GetRequiredService<IDatabaseInitializer>();
                await dbInitializer.SeedAsync(env.ContentRootPath);
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
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
            app.UseHttpsRedirection();
            app.MapControllers();
            app.Run();
        }

        private static AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}
