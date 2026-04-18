using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartInventorySystem.Application.Common;
using SmartInventorySystem.Application.DTOs;
using SmartInventorySystem.Application.Interfaces;
using SmartInventorySystem.Domain.Entities;
using System.Security.Claims;

namespace SmartInventorySystem.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<PagedResult<OrderDto>>>> GetAll([FromQuery] QueryParameters parameters)
        {
            var result = await _orderService.GetAllOrdersAsync(parameters);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<OrderDto>>> GetById(int id)
        {
            var result = await _orderService.GetOrderByIdAsync(id);
            if (!result.Success)
                return NotFound(result);

            // Users can only see their own orders
            var userRole = User.FindFirstValue("Role");
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (userRole != "Admin" && result.Data?.UserId != userId)
                return Forbid();

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<OrderDto>>> Create([FromBody] CreateOrderDto createDto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _orderService.CreateOrderAsync(createDto, userId);

            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<OrderDto>>> UpdateStatus(int id, [FromQuery] OrderStatus status)
        {
            var result = await _orderService.UpdateOrderStatusAsync(id, status);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<ApiResponse<bool>>> Cancel(int id)
        {
            var orderResult = await _orderService.GetOrderByIdAsync(id);
            if (!orderResult.Success)
                return NotFound(orderResult);

            var userRole = User.FindFirstValue("Role");
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (userRole != "Admin" && orderResult.Data?.UserId != userId)
                return Forbid();

            var result = await _orderService.CancelOrderAsync(id);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
