using Microsoft.EntityFrameworkCore;

namespace CodeFirstExample.Domain.Entities
{
    public class Department : StrongEntity<Department>
    {
        [Comment("Tên khoa")]
        public string Name { get; set; }

        public virtual IList<Grade> Grades { get; set; }
    }
}
