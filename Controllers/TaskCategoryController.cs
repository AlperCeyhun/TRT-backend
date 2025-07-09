using Microsoft.AspNetCore.Mvc;
using TRT_backend.Models;
using TRT_backend.Models.DTO;
using TRT_backend.Repositories;

namespace TRT_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskCategoryController : ControllerBase
    {
        private readonly ITaskCategoryRepository _taskCategoryRepository;

        public TaskCategoryController(ITaskCategoryRepository taskCategoryRepository)
        {
            _taskCategoryRepository = taskCategoryRepository;
        }

    
        [Tags("TaskCategoryManagement")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TaskCategoryDto>>> GetAll()
        {
            var categories = await _taskCategoryRepository.GetAllWithTaskCountAsync();
            var categoryDtos = categories.Select(c => new TaskCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Color = c.Color,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            });

            return Ok(categoryDtos);
        }

       
        [Tags("TaskCategoryManagement")]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TaskCategoryDto>> GetById(int id)
        {
            var category = await _taskCategoryRepository.GetByIdWithTasksAsync(id);
            if (category == null)
            {
                return NotFound("Category not found");
            }

            var categoryDto = new TaskCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Color = category.Color,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };

            return Ok(categoryDto);
        }

        
        [Tags("TaskCategoryManagement")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TaskCategoryDto>> Create(CreateTaskCategoryDto createDto)
        {
            if (await _taskCategoryRepository.ExistsByNameAsync(createDto.Name))
            {
                return BadRequest("Category already exists");
            }

            var category = new TaskCategory
            {
                Name = createDto.Name,
                Description = createDto.Description,
                Color = createDto.Color
            };

            await _taskCategoryRepository.AddAsync(category);

            var categoryDto = new TaskCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Color = category.Color,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };

            return CreatedAtAction(nameof(GetById), new { id = category.Id }, categoryDto);
        }

        
        [Tags("TaskCategoryManagement")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TaskCategoryDto>> Update(int id, UpdateTaskCategoryDto updateDto)
        {
            var category = await _taskCategoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound("Category not found");
            }

            if (!string.IsNullOrEmpty(updateDto.Name) && 
                await _taskCategoryRepository.ExistsByNameAsync(updateDto.Name, id))
            {
                return BadRequest("Category already exists");
            }

            if (!string.IsNullOrEmpty(updateDto.Name))
                category.Name = updateDto.Name;
            
            if (updateDto.Description != null)
                category.Description = updateDto.Description;
            
            if (updateDto.Color != null)
                category.Color = updateDto.Color;

            category.UpdatedAt = DateTime.UtcNow;

            await _taskCategoryRepository.UpdateAsync(category);

            var categoryDto = new TaskCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Color = category.Color,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };

            return Ok(categoryDto);
        }

        
        [Tags("TaskCategoryManagement")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(int id)
        {
            var category = await _taskCategoryRepository.GetByIdWithTasksAsync(id);
            if (category == null)
            {
                return NotFound("Category not found");
            }

            if (category.Tasks.Any())
            {
                return BadRequest("This category has tasks, so it cannot be deleted");
            }

            await _taskCategoryRepository.DeleteAsync(id);

            return NoContent();
        }
    }
} 