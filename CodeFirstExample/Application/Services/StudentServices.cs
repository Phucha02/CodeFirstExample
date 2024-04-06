using AutoMapper;
using CodeFirstExample.Application.Dtos;
using CodeFirstExample.Domain.DataContext;
using CodeFirstExample.Domain.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CodeFirstExample.Application.Services
{
    public interface IStudentService : IServices
    {
        public Task<Guid> Create(StudentCreateDto studentCreateDto);
    }

    public class StudentServices : BaseService, IStudentService
    {
        public StudentServices(IMapper mapper)
            : base(mapper)
        {
        }

        public IApplicationDbContext Db { get; set; }

        public async Task<Guid> Create(StudentCreateDto studentCreateDto)
        {
            Student entity = Mapper.Map<Student>(studentCreateDto);

            EntityEntry<Student> result = Db.Students.Add(entity);

            await Db.SaveChangesAsync();

            return result.Entity.Id;
        }
    }
}
