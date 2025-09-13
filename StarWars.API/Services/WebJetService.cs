using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StarWars.Api.Settings;
using StarWars.Common;
using StarWars.Model;
using StarWars.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace StarWars.Api.Services
{
    public interface IWebJetService
    {
        Task<List<MovieView>> GetAllAsync(Provider provider);
        Task<MovieRatingView> GetAsync(Provider provider, string id);
    }

    public class WebJetService : IWebJetService
    {
        private readonly ILogger<WebJetService> _logger;
        private readonly IMemoryCache _cache;
        private readonly IOptions<WebJetSettings> _settings;
        private readonly IHttpClientFactory _httpClientFactory;

        public WebJetService(
            ILogger<WebJetService> logger,
            IMemoryCache cache,
            IOptions<WebJetSettings> settings,
            IHttpClientFactory httpClientFactory)
        {
            (_logger, _cache, _settings, _httpClientFactory) = (
                logger ?? throw new ArgumentNullException(nameof(logger)),
                cache ?? throw new ArgumentNullException(nameof(cache)),
                settings ?? throw new ArgumentNullException(nameof(settings)),
                httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory)));
        }

        public Task<List<MovieView>> GetAllAsync(Provider provider)
        {
            // Cache results per provider
            return _cache.GetOrCreateAsync(provider, async entry => {
                entry.SlidingExpiration = TimeSpan.FromMinutes(_settings.Value.Cache);
                return await CacheAllAsync(provider);
            });
        }

        public async Task<MovieRatingView> GetAsync(Provider provider, string id)
        {
            var httpClient = _httpClientFactory.CreateClient("WebJetClient");
            var response = await httpClient.GetAsync($"{provider}/movie/{id}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Something went wrong: {StatusCode}", response.StatusCode);
                throw new ServiceException(response.StatusCode, $"Get StatusCode: {response.StatusCode}");
            }

            var result = response.Content.ReadAsStringAsync().Result;
            var movieRating = JsonConvert.DeserializeObject<MovieRatingView>(result);

            return movieRating;
        }

        private async Task<List<MovieView>> CacheAllAsync(Provider provider)
        {
            var httpClient = _httpClientFactory.CreateClient("WebJetClient");
            var response = await httpClient.GetAsync($"{provider}/movies");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Something went wrong: {StatusCode}", response.StatusCode);
                throw new ServiceException(response.StatusCode, $"GetAllAsync StatusCode: {response.StatusCode}");
            }

            var jObject = JObject.Parse(await response.Content.ReadAsStringAsync());
            var movies = ((JArray)jObject["Movies"]).Select(o => {
                var movie = JsonConvert.DeserializeObject<Movie>(o.ToString());
                return Mapper.Map<MovieView>(movie);
            }).ToList();

            _logger.LogDebug("Cached {Count} movies from {Provider}", movies.Count, provider);

            return movies;
        }
    }
}
