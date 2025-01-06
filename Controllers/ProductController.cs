using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using URUN.Data;
using URUN.Models;

namespace URUN.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        // Tum urunleri listeleme
        [HttpGet("all")]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var products = await _context.Products
                    .Include(p => p.Category) // Kategori bilgisi dahil edilir
                    .ToListAsync();

                if (!products.Any())
                {
                    return NotFound(new { Mesaj = "Hic urun bulunamadi." });
                }

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mesaj = "Urunleri getirirken bir hata olustu.", Detaylar = ex.Message });
            }
        }

        [HttpPost("add")]
        [Authorize(Roles = "Admin")] // Yalnizca Admin rolune sahip kullanicilar urun ekleyebilir
        public async Task<IActionResult> AddProduct([FromBody] Product product)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Oturum acmis kullanici bilgisine gore CreatedBy'yi atayin
                product.CreatedBy = Guid.NewGuid(); // Burada oturum acmis kullanicinin kimligiyle degistirebilirsiniz

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Mesaj = "Urun eklenirken bir hata olustu.",
                    Detaylar = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        // Belirli bir urunu ID ile getirme
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category) // Kategori bilgisi dahil edilir
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    return NotFound(new { Mesaj = $"ID'si {id} olan urun bulunamadi." });
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mesaj = "Urun getirilirken bir hata olustu.", Detaylar = ex.Message });
            }
        }

        // Urun guncelleme
        [HttpPut("update/{id:int}")]
        [Authorize(Roles = "Admin")] // Yalnizca Admin rolune sahip kullanicilar urun guncelleyebilir
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
        {
            try
            {
                if (id != product.Id)
                {
                    return BadRequest(new { Mesaj = "ID uyusmazligi." });
                }

                var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                if (existingProduct == null)
                {
                    return NotFound(new { Mesaj = $"ID'si {id} olan urun bulunamadi." });
                }

                _context.Entry(product).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(product);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { Mesaj = "Urun guncellenirken bir cakisma olustu." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mesaj = "Urun guncellenirken bir hata olustu.", Detaylar = ex.Message });
            }
        }

        // Urun silme
        [HttpDelete("delete/{id:int}")]
        [Authorize(Roles = "Admin")] // Yalnizca Admin rolune sahip kullanicilar urun silebilir
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound(new { Mesaj = $"ID'si {id} olan urun bulunamadi." });
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return Ok(new { Mesaj = "Urun basariyla silindi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mesaj = "Urun silinirken bir hata olustu.", Detaylar = ex.Message });
            }
        }

        // Urun arama
        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts(
            [FromQuery] string? name,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] int? categoryId
        )
        {
            try
            {
                var query = _context.Products.AsQueryable();

                if (!string.IsNullOrEmpty(name))
                {
                    query = query.Where(p => p.Name.Contains(name));
                }

                if (minPrice.HasValue)
                {
                    query = query.Where(p => p.Price >= minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= maxPrice.Value);
                }

                if (categoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == categoryId.Value);
                }

                var products = await query
                    .Include(p => p.Category)
                    .ToListAsync();

                if (!products.Any())
                {
                    return NotFound(new { Mesaj = "Belirtilen kriterlere uygun urun bulunamadi." });
                }

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mesaj = "Urunler aranirken bir hata olustu.", Detaylar = ex.Message });
            }
        }

        // Korunan Endpoint
        [HttpGet("protected")]
        [Authorize(Roles = "Admin")]  // JWT dogrulamasi gerektirir
        public IActionResult ProtectedEndpoint()
        {
            return Ok(new
            {
                Mesaj = "Bu sayfaya erisim yetkiniz var.",
                Kullanici = User.Identity?.Name
            });
        }
    }
}
