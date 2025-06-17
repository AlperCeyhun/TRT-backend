using Microsoft.AspNetCore.Mvc;
using TRT_backend.Data;
using TRT_backend.Models;

namespace TRT_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TaskController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Create(TodoTask task)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Tasks.Add(task);
            _context.SaveChanges();
            return Ok(task);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var tasks = _context.Tasks.ToList();
            return Ok(tasks);
        }

        [HttpDelete]
        [Route("{id}")]
        public IActionResult Delete(int id)
        {
            var task = _context.Tasks.Find(id);
            if (task == null)
            {
                return NotFound();
            }

            _context.Tasks.Remove(task);
            _context.SaveChanges();
            return Ok();
        }

        [HttpPut]
        [Route("{id}")]
        public IActionResult Update(int id)
        {
            var existingTask = _context.Tasks.Find(id);
            if (existingTask == null)
            {
                return NotFound();
            }

            existingTask.Status = !existingTask.Status;

            _context.SaveChanges();
            return Ok(existingTask);
        }
    }
} 