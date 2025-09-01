using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StarWars.API.Services;
using StarWars.Model;

namespace StarWars.UnitTesting
{
    public class OrderServicePaginationTests
    {
        private OrderService _orderService = null!;

        [SetUp]
        public void Setup()
        {
            var logger = new Mock<ILogger<OrderService>>().Object;
            var jsonOptions = Options.Create(new JsonOptions());
            var cache = new MemoryCache(new MemoryCacheOptions());
            var httpContextAccessor = new Mock<IHttpContextAccessor>().Object;

            _orderService = new OrderService(logger, jsonOptions, cache, httpContextAccessor);
        }

        [Test]
        public async Task Pagination_ShouldReturnStableResults_AcrossRepeatedCalls()
        {
            // Arrange
            var queryParams = new OrderQueryParams {
                Sort = "totalCents",
                Dir = "desc",
                Page = 2,
                PageSize = 10
            };
            var cancellationToken = CancellationToken.None;

            // Act - call service twice with same inputs
            var firstCall = await _orderService.GetAsync(queryParams, cancellationToken);

            var secondCall = await _orderService.GetAsync(queryParams, cancellationToken);

            // Assert - items must be identical
            Assert.That(secondCall.Items.Count(), Is.EqualTo(firstCall.Items.Count()), "Item count mismatch");

            CollectionAssert.AreEqual(
                 firstCall.Items.Select(o => o.Id).ToList(),
                 secondCall.Items.Select(o => o.Id).ToList(),
                 "Pagination results differ between calls");
        }
    }
    public class TestOrderService : OrderService
    {
        public TestOrderService(
            ILogger<OrderService> logger,
            IOptions<JsonOptions> jsonOptions,
            IMemoryCache cache,
            HttpContextAccessor httpContextAccessor) : base(logger, jsonOptions, cache, httpContextAccessor) { }
    }
}
