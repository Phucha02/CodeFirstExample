using AutoMapper;
using CodeFirstExample.Application.Dtos;
using CodeFirstExample.Domain.DataContext;
using CodeFirstExample.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TripleSix.Core.Helpers;

namespace CodeFirstExample.Application.Services
{
    public interface IDepartmentService : IServices
    {
        public Task<Guid> CreateWithMapper(DeparmentCreateDto dto);

        public Task<List<DepartmentDataDto>> GetList(DepartmentFilterDto filter);

        public Task<Guid> UpdateWithMapper(Guid id, DepartmentUpdateDto input);
    }

    public class DepartmentServices : BaseService, IDepartmentService
    {
        public DepartmentServices(IMapper mapper, IApplicationDbContext db)
            : base(mapper)
        {
            Db = db;
        }

        public IApplicationDbContext Db { get; set; }

        public async Task<Guid> CreateWithMapper(DeparmentCreateDto dto)
        {
            var entity = Mapper.Map<DeparmentCreateDto, Department>(dto);
            EntityEntry<Department> result = Db.Departments.Add(entity);
            await Db.SaveChangesAsync();
            return result.Entity.Id;
        }

        public async Task<List<DepartmentDataDto>> GetList(DepartmentFilterDto filter)
        {
            var result = await Db.Departments
                .WhereIf(filter.Id.HasValue, x => x.Id == filter.Id)
                .WhereIf(filter.Name.IsNotNullOrEmpty(), x => x.Name.Contains(filter.Name!))
                .ToListAsync();

            if (result.IsNullOrEmpty())
            {
                return new List<DepartmentDataDto>();
            }

            var list = Mapper.Map<List<Department>, List<DepartmentDataDto>>(result);

            return list;
        }

        public async Task<Guid> UpdateWithMapper(Guid id, DepartmentUpdateDto input)
        {
            var result = await Db.Departments.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (result != null)
            {
                Mapper.Map(input, result);
                EntityEntry<Department> entity = Db.Departments.Update(result);
                await Db.SaveChangesAsync();
                return entity.Entity.Id;
            }
            else
            {
                throw new KeyNotFoundException("Không tìm thấy entity");
            }
        }
    }
}
