using AutoMapper;

namespace CodeFirstExample.Application.CustomMapper.MapperHelper
{
    public static class MapperHelper
    {
        public static IMappingExpression<TSource, TDestination> MapUpdate<TSource, TDestination>(this IMappingExpression<TSource, TDestination> map)
        {
            map.ForAllMembers(source =>
            {
                source.Condition((sourceObject, destObject, sourceProperty, destProperty) =>
                {
                    if (sourceProperty == null)
                        return !(destProperty == null);
                    return !sourceProperty.Equals(destProperty);
                });
            });
            return map;
        }
    }
}
