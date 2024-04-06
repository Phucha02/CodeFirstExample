using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CodeFirstExample.Domain.Entities
{
    public class Student : StrongEntity<Student>
    {
        [Comment("Tên sinh viên")]
        public string Name { get; set; }

        [Comment("Mã sinh viên")]
        public string StudentId { get; set; }

        [Comment("Ngày sinh")]
        public DateTime Dob { get; set; }

        [Comment("Mã lớp")]
        public Guid GradeId { get; set; }

        [ForeignKey(nameof(GradeId))]
        public virtual Grade Grade { get; set; }
    }
}
