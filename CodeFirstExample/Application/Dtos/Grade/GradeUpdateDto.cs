﻿using System.ComponentModel;

namespace CodeFirstExample.Application.Dtos
{
    public class GradeUpdateDto
    {
        [DisplayName("Mã khoa")]
        public Guid? DepartmentId { get; set; }

        [DisplayName("Tên lớp")]
        public string Name { get; set; }
    }
}
