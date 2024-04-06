using AutoMapper;
using CodeFirstExample.Application.Dtos;
using CodeFirstExample.Domain.DataContext;
using CodeFirstExample.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TripleSix.Core.Helpers;

namespace CodeFirstExample.Application.Services
{
    public interface IGradeServices : IServices
    {
        public Task<Guid> CreateWithMapper(GradeCreateDto dto);

        public Task<List<GradeDataDto>> GetList(GradeFilterDto filter);

        public Task<Guid> UpdateWithMapper(Guid id, GradeUpdateDto input);
    }

    public class GradeServices : BaseService, IGradeServices
    {
        public GradeServices(IMapper mapper, IApplicationDbContext db)
            : base(mapper)
        {
            Db = db;
        }

        public IApplicationDbContext Db { get; set; }

        public async Task<Guid> CreateWithMapper(GradeCreateDto dto)
        {
            var entity = Mapper.Map<GradeCreateDto, Grade>(dto);
            EntityEntry<Grade> result = Db.Grades.Add(entity);
            await Db.SaveChangesAsync();
            return result.Entity.Id;
        }

        public async Task<List<GradeDataDto>> GetList(GradeFilterDto filter)
        {
            var result = await Db.Grades
                .WhereIf(filter.DepartmentId.HasValue, x => x.DepartmentId == filter.DepartmentId)
                .WhereIf(filter.Id.HasValue, x => x.Id == filter.Id)
                .WhereIf(filter.Name.IsNotNullOrEmpty(), x => x.Name.Contains(filter.Name!))
                .ToListAsync();

            if (result.IsNullOrEmpty())
            {
                return new List<GradeDataDto>();
            }

            var list = Mapper.Map<List<Grade>, List<GradeDataDto>>(result);

            return list;
        }

        public async Task<Guid> UpdateWithMapper(Guid id, GradeUpdateDto input)
        {
            var result = await Db.Grades.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (result != null)
            {
                Mapper.Map(input, result);
                EntityEntry<Grade> entity = Db.Grades.Update(result);
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
