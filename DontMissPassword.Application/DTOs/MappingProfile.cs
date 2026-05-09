using AutoMapper;
using DontMissPassword.Application.DTOs.AccountDtos;
using DontMissPassword.Application.DTOs.VaultItemDtos;
using DontMissPassword.Domain.Abstractions;
using DontMissPassword.Domain.Entities;

namespace DontMissPassword.Application.DTOs
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //Config Mapper Paging
            CreateMap(typeof(BasePaginatedList<>), typeof(BasePaginatedList<>))
                .ConvertUsing(typeof(BasePaginatedListConverter<,>));

            //
            CreateMap<Account, AccountResponse>();
            CreateMap<VaultItem, ItemResponse>();
        }
        public class BasePaginatedListConverter<TSource, TDestination> : ITypeConverter<BasePaginatedList<TSource>, BasePaginatedList<TDestination>>
        {
            public BasePaginatedList<TDestination> Convert(
                BasePaginatedList<TSource> source,
                BasePaginatedList<TDestination> destination,
                ResolutionContext context)
            {
                var mappedItems = context.Mapper.Map<List<TDestination>>(source.Items);

                return new BasePaginatedList<TDestination>(
                    mappedItems,
                    source.TotalItems,
                    source.PageIndex,
                    source.PageSize
                );
            }
        }
    }
}
