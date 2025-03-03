using Microsoft.AspNetCore.Mvc;
using RestaurantManagementSystem.infrastructure.Data;
using RestaurantManagementSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace RestaurantManagementSystem.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]

public class MenuController : ControllerBase
{
    private readonly RestaurantDbContext _context;

    public MenuController(RestaurantDbContext context)
    {
        _context = context;
    }

    [HttpGet("[Action]")]
    public ActionResult GetAllMenus()
    {
        var menuItems = _context.Menus.ToList();
        return Ok(menuItems);
    }

    [HttpPost("CreateMenu")]
    // [Authorize(Roles = "GeniralStaff")]
    public async Task<IActionResult> CreateMenu([FromBody] Menu menu)
    {
        if (menu == null)
        {
            return BadRequest("Menu ma'lumotlari noto'g'ri.");
        }

        // CategorieId ning mavjudligini tekshirish
        var categoryExists = await _context.Categories.FindAsync(menu.CategorieId);
        if (categoryExists == null)
        {
            return NotFound("Category not found");
        }
        menu.Categorie = null;

        if (menu.Sizes != null && menu.Sizes.Count > 0)
        {
            menu.SizesJson = JsonSerializer.Serialize(menu.Sizes);
        }
        _context.Menus.Add(menu);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(CreateMenu), new { id = menu.Id }, menu);
    }
    [HttpPost("[Action]")]
    [Authorize(Roles = "Staff,GeniralStaff")]
    public async Task<IActionResult> IsPresentNot(int id, bool PutPresent)
    {
        var menu = _context.Menus.FirstOrDefault(o => o.Id == id);
        if (menu == null)
        {
            return NotFound("Menu  topilmadi");
        }
        menu.IsPresent = PutPresent;
        await _context.SaveChangesAsync();
        return Ok(menu);

    }


    [HttpPost("{menuId}/UploadImages")]
    public async Task<IActionResult> UploadImages(int menuId, List<IFormFile> files)
    {
        var menu = await _context.Menus.FindAsync(menuId);
        if (menu == null)
        {
            return NotFound($"Menu ID {menuId} topilmadi.");
        }

        var imageUrls = new List<string>();
        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "menu-images");

        // Agar katalog mavjud bo'lmasa, yarating
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Fayl URLini saqlash
                var imageUrl = $"/menu-images/{fileName}";
                imageUrls.Add(imageUrl);
            }
        }

        // URLlarni menyuga qo'shish
        menu.ImageUrls.AddRange(imageUrls);
        await _context.SaveChangesAsync();

        return Ok(new { menu.Id, UploadedImages = imageUrls });
    }


    [HttpGet("[Action]")]
    [Authorize(Roles = "Staff,GeniralStaff")]
    public IActionResult GetMenuItemById(int id)
    {
        var menuIteam = _context.Menus.FirstOrDefault(m => m.Id == id);

        if (menuIteam == null)
        {
            return NotFound("Menu  topilmadi");
        }

        return Ok(menuIteam);
    }

    [HttpPut("[Action]")]
    // [Authorize(Roles = "Staff,GeniralStaff")]
    public IActionResult UpdateMenuItem(int id, [FromBody] MenuUpdateDto menuUpdateDto)
    {
        var existingMenuItem = _context.Menus.FirstOrDefault(m => m.Id == id);
        if (existingMenuItem == null)
        {
            return NotFound("Menu topilmadi");
        }

        existingMenuItem.Description = menuUpdateDto.Description;
        existingMenuItem.QuantityProduct += menuUpdateDto.QuantityProduct;

        if (existingMenuItem.QuantityProduct > 0)
        {
            existingMenuItem.IsPresent = true;
        }
        else if (existingMenuItem.QuantityProduct <= 0 && existingMenuItem.Considered)
        {
            existingMenuItem.IsPresent = false;
        }

        if (menuUpdateDto.Sizes != null && menuUpdateDto.Sizes.Any())
        {
            existingMenuItem.Sizes = menuUpdateDto.Sizes;
        }

        _context.SaveChanges();

        return Ok(new
        {
            message = "Menu muvaffaqiyatli yangilandi",
            menuId = existingMenuItem.Id,
            updatedFields = new
            {
                description = existingMenuItem.Description,
                quantityProduct = existingMenuItem.QuantityProduct,
                isPresent = existingMenuItem.IsPresent,
                sizes = existingMenuItem.Sizes.Select(s => new { s.Key, s.Value })
            }
        });
    }


    [HttpDelete("[Action]")]
    // [Authorize(Roles = "GeniralStaff")]
    public IActionResult DeleteMenu(int id)
    {
        var menuItem = _context.Menus.FirstOrDefault(m => m.Id == id);
        if (menuItem == null)
        {
            return NotFound("Menu topilmadi");
        }

        _context.Menus.Remove(menuItem);
        _context.SaveChanges();
        return NoContent();
    }


}
