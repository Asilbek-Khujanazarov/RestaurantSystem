using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Domain.Entities;
using RestaurantManagementSystem.infrastructure.Data;

namespace RestaurantManagementSystem.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]

public class CategoryController : ControllerBase
{
    private readonly RestaurantDbContext _context;

    public CategoryController(RestaurantDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Categorie>>> GetCategories()
    {
        return await _context.Categories.ToListAsync();
    }

    // GET: api/Categories/5
    [HttpGet("[Action]")]
    public async Task<ActionResult<Categorie>> GetCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        return category;
    }

    [HttpGet("[Action]")]
    public async Task<ActionResult<IEnumerable<Menu>>> GetCategoryMenus(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Menus)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        return Ok(category.Menus);
    }


    // POST: api/Categories
    [HttpPost]
    public async Task<ActionResult<Categorie>> PostCategory(Categorie categorie)
    {
        _context.Categories.Add(categorie);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategory), new { id = categorie.Id }, categorie);
    }
    [HttpPost("UploadCategoryImage/{id}")]
    public async Task<IActionResult> UploadCategoryImage(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Rasm tanlanmagan.");
        }

        // Kategoriya mavjudligini tekshirish
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound($"Kategoriya {id} topilmadi.");
        }

        // 'wwwroot/images' papkasi mavjudligini tekshirish va yaratish
        var imageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
        if (!Directory.Exists(imageDirectory))
        {
            Directory.CreateDirectory(imageDirectory);  // Papkani yaratish
        }

        // Rasmni serverga saqlash
        var filePath = Path.Combine(imageDirectory, file.FileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Rasm URL'ini kategoriya modeliga qo'shish
        var imageUrl = $"/images/{file.FileName}";
        category.ImageUrl = imageUrl;  // faqat bitta rasm URL'ini saqlash

        // O'zgartirishlarni saqlash
        await _context.SaveChangesAsync();

        return Ok(new { imageUrl });
    }


    // PUT: api/Categories/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCategory(int id, Categorie category)
    {
        if (id != category.Id)
        {
            return BadRequest();
        }

        _context.Entry(category).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CategoryExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/Categories/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool CategoryExists(int id)
    {
        return _context.Categories.Any(e => e.Id == id);
    }
}