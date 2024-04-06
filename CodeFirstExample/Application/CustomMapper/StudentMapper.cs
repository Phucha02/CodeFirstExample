using AutoMapper;
using CodeFirstExample.Application.Dtos;
using CodeFirstExample.Domain.Entities;

namespace CodeFirstExample.Application.CustomMapper
{
    public class StudentMapper : Profile
    {
        public StudentMapper()
        {
            CreateMap<Student, StudentCreateDto>().ReverseMap();
        }
    }
}
