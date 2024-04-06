using AutoMapper;
using CodeFirstExample.Application.CustomMapper.MapperHelper;
using CodeFirstExample.Application.Dtos;
using CodeFirstExample.Domain.Entities;

namespace CodeFirstExample.Application.CustomMapper
{
    public class DepartmentMapper : Profile
    {
        public DepartmentMapper()
        {
            CreateMap<Department, DeparmentCreateDto>().ReverseMap();
            CreateMap<Department, DepartmentDataDto>().ReverseMap();
            CreateMap<DepartmentUpdateDto, Department>().MapUpdate();
        }
    }
}
