using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartInventorySystem.Application.Common;
using SmartInventorySystem.Application.DTOs;
using SmartInventorySystem.Application.Interfaces;
using SmartInventorySystem.Domain.Entities;


namespace SmartInventorySystem.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Manager")]
    public class SuppliersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SuppliersController> _logger;

        public SuppliersController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<SuppliersController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<SupplierDto>>>> GetAll()
        {
            try
            {
                var suppliers = await _unitOfWork.Suppliers.GetAllAsync();
                var supplierDtos = _mapper.Map<IEnumerable<SupplierDto>>(suppliers);
                return Ok(ApiResponse<IEnumerable<SupplierDto>>.SuccessResponse(supplierDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting suppliers");
                return StatusCode(500, ApiResponse<IEnumerable<SupplierDto>>.ErrorResponse("Failed to retrieve suppliers"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<SupplierDto>>> GetById(int id)
        {
            try
            {
                var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
                if (supplier == null)
                    return NotFound(ApiResponse<SupplierDto>.ErrorResponse("Supplier not found"));

                var supplierDto = _mapper.Map<SupplierDto>(supplier);
                return Ok(ApiResponse<SupplierDto>.SuccessResponse(supplierDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supplier by id: {Id}", id);
                return StatusCode(500, ApiResponse<SupplierDto>.ErrorResponse("Failed to retrieve supplier"));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<SupplierDto>>> Create([FromBody] CreateSupplierDto createDto)
        {
            try
            {
                var supplier = _mapper.Map<Supplier>(createDto);
                var createdSupplier = await _unitOfWork.Suppliers.AddAsync(supplier);
                await _unitOfWork.SaveChangesAsync();

                var supplierDto = _mapper.Map<SupplierDto>(createdSupplier);
                return CreatedAtAction(nameof(GetById), new { id = supplierDto.Id },
                    ApiResponse<SupplierDto>.SuccessResponse(supplierDto, "Supplier created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating supplier");
                return StatusCode(500, ApiResponse<SupplierDto>.ErrorResponse("Failed to create supplier"));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<SupplierDto>>> Update(int id, [FromBody] UpdateSupplierDto updateDto)
        {
            if (id != updateDto.Id)
                return BadRequest(ApiResponse<SupplierDto>.ErrorResponse("ID mismatch"));

            try
            {
                var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
                if (supplier == null)
                    return NotFound(ApiResponse<SupplierDto>.ErrorResponse("Supplier not found"));

                _mapper.Map(updateDto, supplier);
                supplier.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Suppliers.UpdateAsync(supplier);
                await _unitOfWork.SaveChangesAsync();

                var supplierDto = _mapper.Map<SupplierDto>(supplier);
                return Ok(ApiResponse<SupplierDto>.SuccessResponse(supplierDto, "Supplier updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supplier");
                return StatusCode(500, ApiResponse<SupplierDto>.ErrorResponse("Failed to update supplier"));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            try
            {
                var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
                if (supplier == null)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Supplier not found"));

                await _unitOfWork.Suppliers.DeleteAsync(supplier);
                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Supplier deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting supplier");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to delete supplier"));
            }
        }
    }
}
