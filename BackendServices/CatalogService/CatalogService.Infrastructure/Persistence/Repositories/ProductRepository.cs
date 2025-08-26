using CatalogService.Application.Repositories;
using CatalogService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Infrastructure.Persistence.Repositories
{
    public class ProductRepository : IProductRepository
    {
        CatalogServiceDbContext _db;
        public ProductRepository(CatalogServiceDbContext catalogServiceDbContext)
        {
            _db = catalogServiceDbContext;
        }
        public void Add(Product product)
        {
            _db.Products.Add(product);
        }

        public void Delete(int id)
        {
            Product product = _db.Products.Find(id);
            if (product != null)
            {
                _db.Products.Remove(product);
            }
        }

        public IEnumerable<Product> GetAll()
        {
            return _db.Products.ToList();
        }

        public Product GetById(int id)
        {
            return _db.Products.Find(id);
        }

        public IEnumerable<Product> GetByIds(int[] ids)
        {
            return _db.Products.Where(p=> ids.Contains(p.ProductId)).ToList();
        }

        public int SaveChanges()
        {
            return _db.SaveChanges();
        }

        public void Update(Product product)
        {
             _db.Products.Update(product);
        }
    }
}
