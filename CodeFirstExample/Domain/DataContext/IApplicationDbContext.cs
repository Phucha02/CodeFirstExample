using CodeFirstExample.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CodeFirstExample.Domain.DataContext
{
    public interface IApplicationDbContext : IDbDataContext
    {
        public DbSet<Department> Departments { get; set; }

        public DbSet<Grade> Grades { get; set; }

        public DbSet<Student> Students { get; set; }
    }
}
