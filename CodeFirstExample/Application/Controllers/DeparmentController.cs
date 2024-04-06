using CodeFirstExample.Application.Dtos;
using CodeFirstExample.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TripleSix.Core.Types;

namespace CodeFirstExample.Application.Controllers
{
    [SwaggerTag("Khoa")]
    [Route("[controller]")]
    public class DeparmentController : Controller
    {
        public DeparmentController(IDepartmentService departmentService)
        {
            DepartmentService = departmentService;
        }

        public IDepartmentService DepartmentService { get; set; }

        [HttpPost]
        [SwaggerOperation("Tạo khoa")]
        public async Task<Guid> Create([FromBody] DeparmentCreateDto input)
        {
            var result = await DepartmentService.CreateWithMapper(input);
            return result;
        }

        [HttpGet("GetList")]
        [SwaggerOperation("Lấy danh sách khoa")]
        public async Task<List<DepartmentDataDto>> GetList([FromQuery] DepartmentFilterDto filter)
        {
            var result = await DepartmentService.GetList(filter);
            return result;
        }

        [HttpPut("{id}")]
        [SwaggerOperation("Cập nhật khoa")]
        public async Task<Guid> Update(RouteId route, [FromBody] DepartmentUpdateDto input)
        {
            var result = await DepartmentService.UpdateWithMapper(route.Id, input);
            return result;
        }
    }
}
