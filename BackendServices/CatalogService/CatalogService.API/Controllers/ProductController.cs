using CatalogService.Application.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.API.Controllers
{

    [Route("api/[controller]/[action]")]
    public class ProductController : ControllerBase
    {

        IProductAppService _productAppService;
        public ProductController(IProductAppService productAppService)
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

    }
}
