using CatalogService.Application.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CatalogController : ControllerBase
    {
        IProductAppService _productAppService;
        public CatalogController(IProductAppService productAppService)
        {
            _productAppService = productAppService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var products = _productAppService.GetAll();
            if (products != null && products.Any())
            {
                return Ok(products);
            }
            return NotFound("No products found.");
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var product = _productAppService.GetById(id);
            if (product != null)
            {
                return Ok(product);
            }
            return NotFound($"Product with ID {id} not found.");
        }

        [HttpPost]
        public IActionResult GetByIds(int[] ids)
        {
            var products = _productAppService.GetByIds(ids);
            if (products != null && products.Any())
            {
                return Ok(products);
            }
            return NotFound("No products found for the provided IDs.");
        }

    }
}
