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
            builder.Services.AddAuthorizationBuilder().AddPolicy("read:messages", 
                policy => policy.Requirements.Add(new HasScopeRequirement("read:messages", issuer)));
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
                c.SwaggerDoc("movies", new OpenApiInfo { Title = "Movies API" });
                c.SwaggerDoc("webjet", new OpenApiInfo { Title = "WebJet API" });
                c.SwaggerDoc("orders", new OpenApiInfo { Title = "Orders API" });
            });

            var app = builder.Build();

            app.UseMiddleware<CorrelationIdMiddleware>();

            using var scope = app.Services.CreateScope();
            var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
            var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
            var dbInitializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();

            // Cache orders from JSON in memory file for soft startup
            await orderService.CacheOrdersAsync(env.ContentRootPath);

            if (app.Environment.IsDevelopment())
            {
                // Seed DB from JSON file for soft startup
                await dbInitializer.SeedAsync(env.ContentRootPath);

                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("CorsPolicy");
            app.UseHttpsRedirection();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.DocumentTitle = "StarWars API";
                c.RoutePrefix = string.Empty;
                c.SwaggerEndpoint("/swagger/movies/swagger.json", "Movies API");
                c.SwaggerEndpoint("/swagger/webjet/swagger.json", "WebJet API");
                c.SwaggerEndpoint("/swagger/orders/swagger.json", "Orders API");
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
