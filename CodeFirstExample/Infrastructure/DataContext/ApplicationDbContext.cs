using CodeFirstExample.Domain.DataContext;
using CodeFirstExample.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CodeFirstExample.Infrastructure.DataContext
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        private readonly IConfiguration _configuration;

        public ApplicationDbContext(IConfiguration configuration)
            : base()
        {
            _configuration = configuration;
        }

        public DbSet<Department> Departments { get; set; }

        public DbSet<Grade> Grades { get; set; }

        public DbSet<Student> Students { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            base.OnConfiguring(builder);

            builder.UseSqlServer(_configuration.GetConnectionString("Default"));
        }
    }
}
