using CatalogService.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Application.Services.Abstractions
{
    public interface IProductAppService
    {
        IEnumerable<ProductDTO> GetAll();
        ProductDTO GetById(int id);
        void Add(ProductDTO product);
        void Update(ProductDTO product);
        void Delete(int id);
        IEnumerable<ProductDTO> GetByIds(int[] ids);

        
    }
}
