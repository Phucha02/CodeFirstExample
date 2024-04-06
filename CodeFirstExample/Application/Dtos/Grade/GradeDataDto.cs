using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace CodeFirstExample.Application.Dtos
{
    public class GradeDataDto
    {
        [DisplayName("Tên lớp")]
        public string Name { get; set; }

        [DisplayName("Mã khoa")]
        public Guid DepartmentId { get; set; }

        [DisplayName("Tên khoa")]
        public string DepartmentName { get; set; }
    }
}
