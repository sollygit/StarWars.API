using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StarWars.API.Services;
using StarWars.Model;
using StarWars.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StarWars.API.Controllers
{
    [ApiController]
    [ApiExplorerSettings(GroupName = "orders")]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ILogger<OrdersController> _logger;
        private readonly IOrderService _orderService;

        public OrdersController(ILogger<OrdersController> logger, IOrderService orderService)
        {
            _logger = logger;
            _orderService = orderService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(OrderResponseView), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAsync(
            [FromQuery] string pharmacyId,
            [FromQuery] List<Status> status,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string sort = Constants.DEFAULT_SORT_BY,
            [FromQuery] string dir = Constants.DEFAULT_DIRECTION,
            [FromQuery] int page = Constants.DEFAULT_PAGE_START,
            [FromQuery] int pageSize = Constants.DEFAULT_PAGE_SIZE,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = new OrderQueryParams {
                    PharmacyId = pharmacyId,
                    Status = status,
                    From = from,
                    To = to,
                    Sort = sort,
                    Dir = dir,
                    Page = page,
                    PageSize = pageSize
                };
                var result = await _orderService.GetAsync(query, cancellationToken);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAsync: Something went wrong");
                return BadRequest(ex.Message);
            }
        }
    }
}
