using SmartInventorySystem.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
namespace SmartInventorySystem.Application.Validators
{

    public class CreateProductValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .MaximumLength(100).WithMessage("Product name must not exceed 100 characters");

            RuleFor(x => x.Sku)
                .NotEmpty().WithMessage("SKU is required")
                .MaximumLength(50).WithMessage("SKU must not exceed 50 characters");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative");

            RuleFor(x => x.MinimumStockThreshold)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum stock threshold cannot be negative");

            RuleFor(x => x.SupplierId)
                .GreaterThan(0).WithMessage("Valid supplier is required");
        }
    }
}
