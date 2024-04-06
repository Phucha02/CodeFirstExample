using AutoMapper;

namespace CodeFirstExample.Application.Services
{
    public interface IServices
    {
    }

    public abstract class BaseService : IServices
    {
        protected BaseService(IMapper mapper)
        {
            Mapper = mapper;
        }

        public IMapper Mapper { get; set; }
    }
}
