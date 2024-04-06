using AutoMapper;
using CodeFirstExample.Application.CustomMapper.MapperHelper;
using CodeFirstExample.Application.Dtos;
using CodeFirstExample.Domain.Entities;

namespace CodeFirstExample.Application.CustomMapper
{
    public class GradeMapper : Profile
    {
        public GradeMapper()
        {
            CreateMap<Grade, GradeCreateDto>().ReverseMap();
            CreateMap<Grade, GradeDataDto>()
                .ForMember(d => d.DepartmentName, o => o.MapFrom((src, dst) => src.Department.Name))
                .ReverseMap();
            CreateMap<GradeUpdateDto, Grade>().MapUpdate();
        }
    }
}
