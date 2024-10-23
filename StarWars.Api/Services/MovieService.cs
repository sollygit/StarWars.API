﻿using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StarWars.Api.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StarWars.Common;
using StarWars.Interface;
using StarWars.Model;
using StarWars.Model.ViewModels;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using StarWars.Repository;

namespace StarWars.Api.Services
{
    public class MovieService : IMovieService
    {
        private readonly ILogger<MovieService> logger;
        private readonly IMemoryCache cache;
        private readonly MovieSettings settings;
        private readonly HttpClient httpClient;
        private readonly IMoviesRepository repo;

        public MovieService(
            ILogger<MovieService> logger,
            IMemoryCache cache,
            MovieSettings settings,
            HttpClient httpClient,
            IMoviesRepository repo)
        {
            this.logger = logger;
            this.cache = cache;
            this.settings = settings;
            this.httpClient = httpClient;
            this.repo = repo;
        }

        public Task<MovieViewModel[]> GetAll(string provider)
        {
            // Cache results per provider
            return cache.GetOrCreateAsync(provider, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(settings.Cache);
                return await GetAllAsync(provider);
            });
        }

        private async Task<MovieViewModel[]> GetAllAsync(string provider)
        {
            // Inject header token
            var uriBuilder = new UriBuilder($"{settings.BaseUrl}/{provider}/movies");
            var response = await httpClient.GetAsync(uriBuilder.Uri);
            var jObject = JObject.Parse(await response.Content.ReadAsStringAsync());
            var movies = ((JArray)jObject["Movies"]).Select(o => {
                var movie = JsonConvert.DeserializeObject<Movie>(o.ToString());
                return Mapper.Map<MovieViewModel>(movie);
            });

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError($"Something went wrong: {response.StatusCode}");
                throw new ServiceException(response.StatusCode, $"Get Movies failed for provider {provider}");
            }

            return movies.ToArray();
        }

        public async Task<MovieDetailsViewModel> Get(string provider, string id)
        {
            var uriBuilder = new UriBuilder($"{settings.BaseUrl}/{provider}/movie/{id}");
            var response = await httpClient.GetAsync(uriBuilder.Uri);
            var result = response.Content.ReadAsStringAsync().Result;
            var movie = JsonConvert.DeserializeObject<MovieDetails>(result);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError($"Something went wrong: {response.StatusCode}");
                throw new ServiceException(response.StatusCode, $"Get Movie failed for provider {provider} and id {id}");
            }

            return Mapper.Map<MovieDetailsViewModel>(movie);
        }

        public async Task<MovieViewModel> Get(Guid movieID)
        {
            var movie = await repo.Get(movieID);
            return Mapper.Map<MovieViewModel>(movie);
        }
    }
}
