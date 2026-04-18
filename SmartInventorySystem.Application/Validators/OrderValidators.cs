using FluentValidation;
using SmartInventorySystem.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
namespace SmartInventorySystem.Application.Validators
{


    public class CreateOrderValidator : AbstractValidator<CreateOrderDto>
    {
        public CreateOrderValidator()
        {
            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("Order must contain at least one item");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId)
                    .GreaterThan(0).WithMessage("Valid product is required");

                item.RuleFor(i => i.Quantity)
                    .GreaterThan(0).WithMessage("Quantity must be greater than 0");
            });
        }
    }
}
