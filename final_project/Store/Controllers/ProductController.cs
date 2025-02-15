using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string search = null,
        [FromQuery] string category = "all",
        [FromQuery] string company = "all",
        [FromQuery] string order = "a-z",
        [FromQuery] int? price = null,
        [FromQuery] string shipping = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var filters = new ProductFilters
        {
            Search = search,
            Category = category,
            Company = company,
            Order = order,
            Price = price,
            Shipping = shipping?.ToLower() == "on"
        };

         var (products, total) = await _productService.GetFilteredProductsAsync(filters, page, pageSize);
        
        var categories = _productService.GetCategories();
    var companies = _productService.GetCompanies();

        return Ok(new
        {
            data = products.Select(p => new
            {
                id = p.Id,
                attributes = new
                {
                    title = p.Title,
                    company = p.Company,
                    description = p.Description,
                    featured = p.Featured,
                    createdAt = p.CreatedAt,
                    updatedAt = p.UpdatedAt,
                    category = p.Category,
                    image = p.Image,
                    price = p.Price,
                    shipping = p.Shipping,
                    colors = p.Colors
                }
            }),
            meta = new
            {
                pagination = new
                {
                    page,
                    pageSize,
                    pageCount = (int)Math.Ceiling(total / (double)pageSize),
                    total
                },
                categories,
                companies
            }
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);

        if (product == null)
        {
            return NotFound(new { message = "Product not found." });
        }

        return Ok(new
        {
            data = new
            {
                id = product.Id,
                attributes = new
                {
                    title = product.Title,
                    company = product.Company,
                    description = product.Description,
                    featured = product.Featured,
                    createdAt = product.CreatedAt,
                    updatedAt = product.UpdatedAt,
                    category = product.Category,
                    image = product.Image,
                    price = product.Price,
                    shipping = product.Shipping,
                    colors = product.Colors
                }
            },
            meta = new { }
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] Product product)
    {
        await _productService.CreateProductAsync(product);
        return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
    }
}
