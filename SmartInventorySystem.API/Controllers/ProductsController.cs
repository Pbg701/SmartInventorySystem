using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using SmartInventorySystem.Application.Common;
using SmartInventorySystem.Application.DTOs;
using SmartInventorySystem.Application.Interfaces;
using SmartInventorySystem.Application.Services;

namespace SmartInventorySystem.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<ProductDto>>>> GetAll([FromQuery] QueryParameters parameters)
        {
            var result = await _productService.GetAllProductsAsync(parameters);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(int id)
        {
            var result = await _productService.GetProductByIdAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("low-stock")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetLowStock()
        {
            var result = await _productService.GetLowStockProductsAsync();
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> Create([FromBody] CreateProductDto createDto)
        {
            var result = await _productService.CreateProductAsync(createDto);
            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> Update(int id, [FromBody] UpdateProductDto updateDto)
        {
            if (id != updateDto.Id)
                return BadRequest(ApiResponse<ProductDto>.ErrorResponse("ID mismatch"));

            var result = await _productService.UpdateProductAsync(updateDto);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }
    }
}
