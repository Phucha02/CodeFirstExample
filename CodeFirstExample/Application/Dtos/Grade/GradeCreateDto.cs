using System.ComponentModel;

namespace CodeFirstExample.Application.Dtos
{
    public class GradeCreateDto
    {
        [DisplayName("Tên lớp")]
        public string Name { get; set; }

        [DisplayName("Mã khoa")]
        public Guid DepartmentId { get; set; }
    }
}
