using AutoMapper;
using CourseManager.API.Models;
using CourseManager.API.DTOs;

namespace CourseManager.API.Profiles
{
    public class CourseProfile : Profile
    {
        public CourseProfile()
        {
            // Course to CourseDto
            CreateMap<Course, CourseDto>()
                .ForMember(dest => dest.DiscountPercentage, opt => opt.MapFrom(src => 
                    src.OriginalPrice.HasValue && src.OriginalPrice > 0 
                        ? Math.Round((1 - src.Price / src.OriginalPrice.Value) * 100, 0)
                        : 0))
                .ForMember(dest => dest.IsOnSale, opt => opt.MapFrom(src => 
                    src.OriginalPrice.HasValue && src.OriginalPrice > src.Price));

            // CourseDto to Course
            CreateMap<CourseDto, Course>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.OrderItems, opt => opt.Ignore());

            // CreateCourseRequest to Course
            CreateMap<CreateCourseRequest, Course>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.OrderItems, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            // UpdateCourseRequest to Course
            CreateMap<UpdateCourseRequest, Course>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.OrderItems, opt => opt.Ignore());
        }
    }
}
