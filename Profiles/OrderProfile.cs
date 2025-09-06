using AutoMapper;
using CourseManager.API.Models;
using CourseManager.API.DTOs;

namespace CourseManager.API.Profiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            // Order to OrderDto
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.OrderItems.Sum(oi => oi.Quantity)));

            // OrderDto to Order
            CreateMap<OrderDto, Order>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.OrderItems, opt => opt.Ignore());

            // CreateOrderRequest to Order
            CreateMap<CreateOrderRequest, Order>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrderNumber, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.OrderItems, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => "Pending"))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending"));

            // OrderItem to OrderItemDto
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.CourseTitle, opt => opt.MapFrom(src => src.CourseTitle))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.Price * src.Quantity));

            // OrderItemDto to OrderItem
            CreateMap<OrderItemDto, OrderItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Order, opt => opt.Ignore())
                .ForMember(dest => dest.Course, opt => opt.Ignore());

            // CreateOrderItemRequest to OrderItem
            CreateMap<CreateOrderItemRequest, OrderItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Order, opt => opt.Ignore())
                .ForMember(dest => dest.Course, opt => opt.Ignore())
                .ForMember(dest => dest.CourseTitle, opt => opt.Ignore())
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Instructor, opt => opt.Ignore());
        }
    }
}
