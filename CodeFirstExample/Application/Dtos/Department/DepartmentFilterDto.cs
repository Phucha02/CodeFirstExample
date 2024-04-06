using System.ComponentModel;

namespace CodeFirstExample.Application.Dtos
{
    public class DepartmentFilterDto
    {
        [DisplayName("Mã khoa")]
        public Guid? Id { get; set; }

        [DisplayName("Tên khoa")]
        public string? Name { get; set; }
    }
}
