using Microsoft.AspNetCore.Mvc;
using TRT_backend.Data;
using TRT_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace TRT_backend.Controllers
{
    [ApiController]
    [Route("api/todo-tasks")]
    public class TaskController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TaskController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Create([FromBody] CreateTaskDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var task = new TodoTask
            {
                Title = dto.Title,
                Description = dto.Description,
                Category = dto.Category,
                Completed = dto.Completed
            };
            _context.Tasks.Add(task);
            _context.SaveChanges();

            return Ok(task);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var tasks = _context.Tasks
                .Include(t => t.Assignees)
                .ThenInclude(a => a.User)
                .Select(t => new {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Category,
                    t.Completed,
                    Assignees = t.Assignees.Select(a => new {
                        a.UserId,
                        a.User.username
                    }).ToList()
                })
                .ToList();

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
        public IActionResult Update(int id, [FromBody] UpdateTaskDto updates)
        {
            var existingTask = _context.Tasks.Find(id);
            if (existingTask == null)
            {
                return NotFound();
            }

            existingTask.Title = updates.Title;
            existingTask.Description = updates.Description;
            existingTask.Category = updates.Category;
            existingTask.Completed = updates.Completed;

            _context.SaveChanges();
            return Ok(existingTask);
        }

        public class UpdateTaskDto
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public TaskCategory Category { get; set; }
            public bool Completed { get; set; }
        }

        public class CreateTaskDto
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public TaskCategory Category { get; set; }
            public bool Completed { get; set; }
        }
    }
} 