using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using StarWars.Api.Services;
using StarWars.Api.Settings;
using StarWars.Interface;
using StarWars.Model;
using StarWars.Repository;
using System;
using System.Net.Http;
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
                    builder => builder.WithOrigins(Configuration["CorsUrl"])
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });

            Mapper.Initialize(cfg => {
                cfg.AddProfile<AutoMapperProfile>();
            });

            var settings = Configuration.GetSection("WebJetSettings");

            // Configurations
            services.Configure<WebJetSettings>(settings);
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

            services.AddSingleton(provider => settings.Get<WebJetSettings>());
            services.AddTransient<IMoviesRepository, MovieRepository>();
            services.AddScoped<IMovieService, MovieService>();
            services.AddScoped<IWebJetService, WebJetService>();
            services.AddHttpClient<IWebJetService, WebJetService>(client => {
                client.DefaultRequestHeaders.Add("x-access-token", settings["AccessToken"]);
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(5)) // Set lifetime to five minutes
            .AddPolicyHandler(GetRetryPolicy()); // Set retry policy

            services
                .AddRouting(o => o.LowercaseUrls = true)
                .AddControllers()
                .AddJsonOptions(o => o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

            services.AddFluentValidationAutoValidation();
            services.AddFluentValidationClientsideAdapters();
            services.AddValidatorsFromAssemblyContaining<MovieValidator>(ServiceLifetime.Transient);

            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "StarWars API", Version = "v1" });
            });
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
