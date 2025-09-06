using AutoMapper;
using CourseManager.API.Models;
using CourseManager.API.DTOs;

namespace CourseManager.API.Profiles
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            // Category to CategoryDto
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.CourseCount, opt => opt.MapFrom(src => src.Courses.Count));

            // CategoryDto to Category
            CreateMap<CategoryDto, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Courses, opt => opt.Ignore());

            // CreateCategoryRequest to Category
            CreateMap<CreateCategoryRequest, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Courses, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            // UpdateCategoryRequest to Category
            CreateMap<UpdateCategoryRequest, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Courses, opt => opt.Ignore());
        }
    }
}
