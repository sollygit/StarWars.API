using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StarWars.Model;
using StarWars.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace StarWars.API.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> CacheOrdersAsync(string rootPath);
        Task<OrderResponseView> GetAsync(OrderQueryParams query, CancellationToken cancellationToken = default);
    }

    public class OrderService : IOrderService
    {
        private readonly ILogger<OrderService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IEnumerable<Order> _orders = [];

        public OrderService(
            ILogger<OrderService> logger,
            IOptions<JsonOptions> jsonOptions,
            IMemoryCache cache,
            IHttpContextAccessor httpContextAccessor)
        {
            (_logger, _jsonOptions, _cache, _httpContextAccessor) = (
                logger ?? throw new ArgumentNullException(nameof(logger)),
                jsonOptions?.Value?.JsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonOptions)),
                cache ?? throw new ArgumentNullException(nameof(cache)),
                httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor)));
        }

        public async Task<IEnumerable<Order>> CacheOrdersAsync(string rootPath)
        {
            return await _cache.GetOrCreateAsync("Orders", async e =>
            {
                e.SetOptions(new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60) });

                var stopwatch = Stopwatch.StartNew();
                _orders = await LoadOrdersAsync(rootPath);
                stopwatch.Stop();

                _logger.LogDebug("Cached {Total} orders in {Duration} ms.", _orders.Count(), stopwatch.Elapsed.TotalMilliseconds);

                return _orders ?? [];
            }) ?? [];
        }

        public Task<OrderResponseView> GetAsync(OrderQueryParams query, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            // Check if cancellation has been requested
            cancellationToken.ThrowIfCancellationRequested();

            // Validate query parameters
            ValidateQueryParams(query);

            // Get all orders
            var items = _orders.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(query.PharmacyId))
                items = items.Where(o => o.PharmacyId.Equals(query.PharmacyId, StringComparison.OrdinalIgnoreCase));
            if (query.Status != null && query.Status.Count > 0)
                items = items.Where(o => query.Status.Contains(o.Status));
            if (query.From.HasValue)
                items = items.Where(o => o.CreatedAt >= query.From.Value);
            if (query.To.HasValue)
                items = items.Where(o => o.CreatedAt <= query.To.Value);

            // Apply sorting and direction
            items = (query.Sort, query.Dir) switch
            {
                ("createdAt", "desc") => items.OrderByDescending(o => o.CreatedAt),
                ("createdAt", "asc") => items.OrderBy(o => o.CreatedAt),
                ("totalCents", "desc") => items.OrderByDescending(o => o.TotalCents),
                ("totalCents", "asc") => items.OrderBy(o => o.TotalCents),
                _ => items.OrderByDescending(o => o.CreatedAt) // Default sort by createdAt desc
            };

            // Apply pagination
            items = items.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize);

            stopwatch.Stop();

            // Log one structured line per request with correlation ID, validated params, elapsed ms and item count
            _logger.LogDebug("GetAsync: {CorrelationId}, {QueryParams}, {Duration} ms, {ItemCount} items.",
                _httpContextAccessor.HttpContext?.Items[Constants.X_CORRELATION_ID],
                query.ToString(),
                stopwatch.Elapsed.TotalMilliseconds,
                items.Count());

            return Task.FromResult(new OrderResponseView
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                Total = items.Count()
            });
        }

        private async Task<IEnumerable<Order>> LoadOrdersAsync(string rootPath)
        {
            var path = Path.Combine(rootPath, "orders.json");

            try
            {
                var json = await File.ReadAllTextAsync(path);
                var orders = JsonSerializer.Deserialize<IEnumerable<Order>>(json, _jsonOptions) ?? [];

                // Apply NeedsReview business rule here
                return orders.Select(o =>
                {
                    o.NeedsReview = o.TotalCents > 2000;
                    return o;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load orders from {Path}", path);
                return [];
            }
        }

        private static void ValidateQueryParams(OrderQueryParams query)
        {
            if (query.Page <= 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(query)}.{nameof(query.Page)}", query.Page, "Page must be greater than 0.");
            }
            if (query.PageSize <= 0 || query.PageSize > Constants.MAX_PAGE_SIZE)
            {
                throw new ArgumentOutOfRangeException($"{nameof(query)}.{nameof(query.PageSize)}", query.PageSize, $"PageSize must be between 1 and {Constants.MAX_PAGE_SIZE}.");
            }
            if (query.Sort != "createdAt" && query.Sort != "totalCents")
            {
                throw new ArgumentException($"Invalid Sort '{query.Sort}'. Sort must be either 'createdAt' or 'totalCents'.", $"{nameof(query)}.{nameof(query.Sort)}");

            }
            if (query.Dir != "desc" && query.Dir != "asc")
            {
                throw new ArgumentException($"Invalid Dir '{query.Dir}'. Dir must be either 'desc' or 'asc'.", $"{nameof(query)}.{nameof(query.Dir)}");
            }
        }
    }
}
