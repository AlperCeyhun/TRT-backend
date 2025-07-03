using Microsoft.AspNetCore.Mvc;
using TRT_backend.Data;
using TRT_backend.Models;
using System.Threading.Tasks;
using System.Linq;

namespace TRT_backend.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        [Tags("CategoryManagement")]
        [HttpGet]
        public IActionResult GetAll()
        {
            var categories = _context.Categories.ToList();
            return Ok(categories);
        }

        [Tags("CategoryManagement")]
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
                return NotFound();
            return Ok(category);
        }
        [Tags("CategoryManagement")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
                return BadRequest("Kategori adı boş olamaz.");
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return Ok(category);
        }

        [Tags("CategoryManagement")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Category updated)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
                return NotFound();
            if (string.IsNullOrWhiteSpace(updated.Name))
                return BadRequest("Kategori adı boş olamaz.");
            category.Name = updated.Name;
            await _context.SaveChangesAsync();
            return Ok(category);
        }
        [Tags("CategoryManagement")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
                return NotFound();
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
} 