using Microsoft.AspNetCore.Mvc;
using TRT_backend.Services;
using TRT_backend.Models;
using Microsoft.AspNetCore.Authorization;
using TRT_backend.Models.DTO;
using TRT_backend.Repositories;
using System.Text.Json;
using System.Text;

namespace TRT_backend.Controllers
{
    [ApiController]
    [Route("api/todo-tasks")]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly IUserService _userService;
        private readonly ITaskCategoryRepository _taskCategoryRepository;

        private readonly HttpClient _httpClient;
        private readonly string _geminiApiKey = "AIzaSyAVK_w9msCcwJGL4jz69U8-i6t-JRQf6i8";


        public TaskController(ITaskService taskService, IUserService userService, ITaskCategoryRepository taskCategoryRepository,HttpClient httpClient)
        {
            _taskService = taskService;
            _userService = userService;
            _httpClient = httpClient;
            _taskCategoryRepository = taskCategoryRepository;
        }
        [EndpointSummary("CreateTask")]
        [Tags("TaskManagement")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskDto dto)
        {
            var userId = _userService.GetUserIdFromToken(User);
            
            if (!_userService.HasUserPermissionFromToken(User, "AddTask"))
                return StatusCode(403, "You don't have permission to add task.");

            // Kategori kontrolü
            if (dto.CategoryId.HasValue)
            {
                var category = await _taskCategoryRepository.GetByIdAsync(dto.CategoryId.Value);
                if (category == null)
                    return BadRequest("Invalid category ID");
            }

            var task = new TodoTask
            {
                Title = dto.Title,
                Description = dto.Description,
                Completed = dto.Completed,
                CategoryId = dto.CategoryId
            };

            var createdTask = await _taskService.CreateTaskAsync(task);

           
            await _taskService.AssignUserToTaskAsync(createdTask.Id, userId);

            var estimatedDuration = await GetEstimatedDurationFromGemini(dto);

            var result = new
            {
                createdTask.Id,
                createdTask.Title,
                createdTask.Description,
                createdTask.Completed,
                EstimatedDuration = estimatedDuration
            };

            return Ok(result);
        }

        private async Task<string> GetEstimatedDurationFromGemini(CreateTaskDto dto)
        {
            var prompt = $@" 
            Bir görev atandı.Görev bilgileri :
            Görev Adı: {dto.Title}
            Acıklama : {dto.Description}
            Aciliyet : {(dto.CategoryId.HasValue ? dto.CategoryId.ToString() : "bilinmyor")}

            Bu görevin ortalama tamamlanma süresi kaç saat/gün olur? Sadece süreyi belirt.";

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={_geminiApiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new {
                        parts = new[] {
                            new { text = prompt }
                        }
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return "Tahmin yapılamadı";

            using var doc = JsonDocument.Parse(json);
            return doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();


        }
        [EndpointSummary("GetTasks")]
        [Tags("TaskManagement")]
        [HttpGet]
        public async Task<IActionResult> GetTasks(int pageNumber = 1, int pageSize = 2)
        {
            
            if (_userService.CanViewAllTasksFromToken(User))
            {
                var allTasks = await _taskService.GetAllTasksAsync();
                var pagedTasks = allTasks
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new
                    {
                        t.Id,
                        t.Title,
                        t.Description,
                        t.Completed,
                        Category = t.Category != null ? new
                        {
                            t.Category.Id,
                            t.Category.Name,
                            t.Category.Color
                        } : null,
                        Assignees = t.Assignees.Select(a => new
                        {
                            a.Id,
                            UserId = a.User.Id,
                            Username = a.User.username
                        }).ToList()
                    }).ToList();

                return Ok(new
                {
                    Data = pagedTasks,
                    TotalCount = allTasks.Count,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                });
            }
            else
            {
                var tasks = await _taskService.GetTasksForUserAsync(User, pageNumber, pageSize);
                var totalCount = await _taskService.GetTotalTaskCountAsync(User);

                var pagedTasks = tasks.Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Completed,
                    Category = t.Category != null ? new
                    {
                        t.Category.Id,
                        t.Category.Name,
                        t.Category.Color
                    } : null,
                    Assignees = t.Assignees.Select(a => new
                    {
                        a.Id,
                        UserId = a.User.Id,
                        Username = a.User.username
                    }).ToList()
                }).ToList();

                return Ok(new
                {
                    Data = pagedTasks,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                });
            }
        }
        [EndpointSummary("DeleteTask")]
        [Tags("TaskManagement")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userService.GetUserIdFromToken(User);
            
            if (!_userService.HasUserPermissionFromToken(User, "DeleteTask"))
                return StatusCode(403, "You are not authorized to perform this operation.");

            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
                return NotFound();

           
            if (!_userService.IsUserAdminFromToken(User))
            {
                var assignedTaskIds = _userService.GetAssignedTaskIdsFromToken(User);
                if (!assignedTaskIds.Contains(id))
                    return StatusCode(403, "You can only delete the task to which you are assigned.");
            }

            var success = await _taskService.DeleteTaskAsync(id);
            if (!success)
                return NotFound();

            return Ok();
        }
        [EndpointSummary("UpdateTask")]
        [Tags("TaskManagement")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskDto updates)
        {
            var userId = _userService.GetUserIdFromToken(User);
            
            var existingTask = await _taskService.GetTaskByIdAsync(id);
            if (existingTask == null)
                return NotFound();

           
            if (!_userService.IsUserAdminFromToken(User))
            {
                var assignedTaskIds = _userService.GetAssignedTaskIdsFromToken(User);
                if (!assignedTaskIds.Contains(id))
                    return StatusCode(403, "You can only update the task you are assigned to.");
            }

            
            if (!_userService.IsUserAdminFromToken(User))
            {
                if (updates.Title != existingTask.Title && 
                    !_userService.HasUserPermissionFromToken(User, "EditTaskTitle"))
                    return StatusCode(403, "You don't have permission to edit task title.");
                
                if (updates.Description != existingTask.Description && 
                    !_userService.HasUserPermissionFromToken(User, "EditTaskDescription"))
                    return StatusCode(403, "You don't have permission to edit task description.");
                
                if (updates.Completed != existingTask.Completed && 
                    !_userService.HasUserPermissionFromToken(User, "EditTaskStatus"))
                    return StatusCode(403, "You don't have permission to edit task status.");
            }

           
            // Kategori kontrolü
            if (updates.CategoryId.HasValue && updates.CategoryId != existingTask.CategoryId)
            {
                var category = await _taskCategoryRepository.GetByIdAsync(updates.CategoryId.Value);
                if (category == null)
                    return BadRequest("Geçersiz kategori ID");
            }

            if (updates.Title != existingTask.Title)
                existingTask.Title = updates.Title;
            if (updates.Description != existingTask.Description)
                existingTask.Description = updates.Description;
            if (updates.Completed != existingTask.Completed)
                existingTask.Completed = updates.Completed;
            if (updates.CategoryId != existingTask.CategoryId)
                existingTask.CategoryId = updates.CategoryId;

            var updatedTask = await _taskService.UpdateTaskAsync(id, existingTask);
            if (updatedTask == null)
                return NotFound();

           
            var result = new
            {
                updatedTask.Id,
                updatedTask.Title,
                updatedTask.Description,
                updatedTask.Completed,
                Category = updatedTask.Category != null ? new
                {
                    updatedTask.Category.Id,
                    updatedTask.Category.Name,
                    updatedTask.Category.Color
                } : null
            };

            return Ok(result);
        }
    }
} 