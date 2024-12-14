using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.infrastructure.Data;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly RestaurantDbContext _context;
    public WarehouseController(RestaurantDbContext context)
    {
        _context = context;
    }

    [HttpGet("[Action]")]
    [Authorize(Roles = "GeniralStaff")]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _context.Products.ToListAsync();
        return Ok(products);
    }

    [HttpPost("[Action]")]
    public async Task<IActionResult> AddProduct([FromBody] Product product)
    {
        _context.Products.Add(product);
        product.OldQuantity = product.Quantity;
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product);
    }

    [HttpPut("[Action]")]
    [Authorize(Roles = "Staff,GeniralStaff")]
    public async Task<IActionResult> AddQuantity(int id, [FromBody] double quantity)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        product.Quantity += quantity;
        product.OldQuantity = product.Quantity;
        await _context.SaveChangesAsync();
        return Ok(product);
    }

    [HttpPut("[Action]")]
    [Authorize(Roles = "Staff,GeniralStaff")]
    public async Task<IActionResult> SubtractQuantity(int id, [FromBody] double quantity)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        if (product.Quantity < quantity)
            return BadRequest("Not enough quantity in stock.");

        product.Quantity -= quantity;
        await _context.SaveChangesAsync();
        return Ok(product);
    }
    [HttpDelete("[Action]")]
    [Authorize(Roles = "GeniralStaff")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound("Maxsulot topilmadi");
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return Ok("Maxsulot o'chirildi");
    }

    // 10% dan  kam  qolganlar
    [HttpGet("[Action]")]
    [Authorize(Roles = "Staff,GeniralStaff")]
    public async Task<ActionResult<IEnumerable<Product>>> GetLowStockProducts()
    {
        var lowStockProducts = await _context.Products
            .Where(p => p.Quantity < (p.OldQuantity * 0.1))
            .ToListAsync();

        if (!lowStockProducts.Any())
            return NotFound("10% dan kam mahsulotlar topilmadi.");

        return Ok(lowStockProducts);
    }

    // Tugagan mahsulotlarni olish
    [HttpGet("[Action]")]
    [Authorize(Roles = "Staff,GeniralStaff")]
    
    public async Task<ActionResult<IEnumerable<Product>>> GetOutOfStockProducts()
    {
        var outOfStockProducts = await _context.Products
            .Where(p => p.Quantity == 0)
            .ToListAsync();

        if (!outOfStockProducts.Any())
            return NotFound("Tugagan mahsulotlar topilmadi.");

        return Ok(outOfStockProducts);
    }



}