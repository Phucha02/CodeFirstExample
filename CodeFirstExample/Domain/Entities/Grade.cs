using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeFirstExample.Domain.Entities
{
    public class Grade : StrongEntity<Grade>
    {
        [Comment("Tên lớp")]
        public string Name { get; set; }

        [Comment("Khoa")]
        public Guid DepartmentId { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public virtual Department Department { get; set; }

        public virtual IList<Student> Students { get; set; }

        public override void Configure(EntityTypeBuilder<Grade> builder)
        {
            base.Configure(builder);
            builder.HasIndex(d => d.Name).IsUnique();
        }
    }
}
