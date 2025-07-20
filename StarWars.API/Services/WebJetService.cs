using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StarWars.Api.Settings;
using StarWars.Common;
using StarWars.Interface;
using StarWars.Model;
using StarWars.Model.ViewModels;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace StarWars.Api.Services
{
    public class WebJetService : IWebJetService
    {
        private readonly ILogger<WebJetService> _logger;
        private readonly IMemoryCache _cache;
        private readonly WebJetSettings _settings;
        private readonly IHttpClientFactory _httpClientFactory;

        public WebJetService(
            ILogger<WebJetService> logger,
            IMemoryCache cache,
            WebJetSettings settings,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _cache = cache;
            _settings = settings;
            _httpClientFactory = httpClientFactory;
        }

        public Task<MovieView[]> GetAll(string provider)
        {
            // Cache results per provider
            return _cache.GetOrCreateAsync(provider, async entry => {
                entry.SlidingExpiration = TimeSpan.FromMinutes(_settings.Cache);
                return await GetAllAsync(provider);
            });
        }

        private async Task<MovieView[]> GetAllAsync(string provider)
        {
            var httpClient = _httpClientFactory.CreateClient("WebJetClient");
            var response = await httpClient.GetAsync($"{provider}/movies");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Something went wrong: {response.StatusCode}");
                throw new ServiceException(response.StatusCode, $"GetAllAsync StatusCode: {response.StatusCode}");
            }

            var jObject = JObject.Parse(await response.Content.ReadAsStringAsync());
            var movies = ((JArray)jObject["Movies"]).Select(o => {
                var movie = JsonConvert.DeserializeObject<Movie>(o.ToString());
                return Mapper.Map<MovieView>(movie);
            });

            return movies.ToArray();
        }

        public async Task<MovieRatingView> Get(string provider, string id)
        {
            var httpClient = _httpClientFactory.CreateClient("WebJetClient");
            var response = await httpClient.GetAsync($"{provider}/movie/{id}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Something went wrong: {response.StatusCode}");
                throw new ServiceException(response.StatusCode, $"Get StatusCode: {response.StatusCode}");
            }

            var result = response.Content.ReadAsStringAsync().Result;
            var movieRating = JsonConvert.DeserializeObject<MovieRatingView>(result);

            return movieRating;
        }
    }
}
