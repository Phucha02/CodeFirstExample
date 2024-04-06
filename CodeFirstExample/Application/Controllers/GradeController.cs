using CodeFirstExample.Application.Dtos;
using CodeFirstExample.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TripleSix.Core.Types;

namespace CodeFirstExample.Application.Controllers
{
    [SwaggerTag("Lớp")]
    [Route("[controller]")]
    public class GradeController : Controller
    {
        public GradeController(IGradeServices gradeServices)
        {
            GradeServices = gradeServices;
        }

        public IGradeServices GradeServices { get; set; }

        [HttpPost]
        [SwaggerOperation("Tạo lớp")]
        public async Task<Guid> Create([FromBody] GradeCreateDto input)
        {
            var result = await GradeServices.CreateWithMapper(input);
            return result;
        }

        [HttpGet("GetList")]
        [SwaggerOperation("Lấy danh sách lớp")]
        public async Task<List<GradeDataDto>> GetList([FromQuery] GradeFilterDto filter)
        {
            var result = await GradeServices.GetList(filter);
            return result;
        }

        [HttpPut("{id}")]
        [SwaggerOperation("Cập nhật lớp")]
        public async Task<Guid> Update(RouteId route, [FromBody] GradeUpdateDto input)
        {
            var result = await GradeServices.UpdateWithMapper(route.Id, input);
            return result;
        }
    }
}
