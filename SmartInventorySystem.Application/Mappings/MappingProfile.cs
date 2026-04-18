using SmartInventorySystem.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using AutoMapper;
using SmartInventorySystem.Domain.Entities;
namespace SmartInventorySystem.Application.Mappings
{

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : string.Empty))
                .ForMember(dest => dest.IsLowStock, opt => opt.MapFrom(src => src.IsLowStock));

            CreateMap<CreateProductDto, Product>();
            CreateMap<UpdateProductDto, Product>();

            CreateMap<Supplier, SupplierDto>();
            CreateMap<CreateSupplierDto, Supplier>();
            CreateMap<UpdateSupplierDto, Supplier>();

            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty));

            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Subtotal));

            CreateMap<ApplicationUser, UserDto>();
        }
    }
}
