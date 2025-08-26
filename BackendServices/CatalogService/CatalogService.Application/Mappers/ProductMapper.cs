using AutoMapper;
using CatalogService.Application.DTO;
using CatalogService.Domain.Entities;

namespace CatalogService.Application.Mappers
{
    public class ProductMapper:Profile
    {
        public ProductMapper()
        {
            CreateMap<Product,ProductDTO>().ReverseMap();
        }
    }
}
