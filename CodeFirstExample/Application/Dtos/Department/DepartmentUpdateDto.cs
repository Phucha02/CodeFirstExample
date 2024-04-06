using System.ComponentModel;

namespace CodeFirstExample.Application.Dtos
{
    public class DepartmentUpdateDto
    {
        [DisplayName("Tên khoa")]
        public string Name { get; set; }
    }
}
