using System.ComponentModel.DataAnnotations;

namespace TRT_backend.Models
{
    public class TaskCategory
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public string? Color { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual ICollection<TodoTask> Tasks { get; set; } = new List<TodoTask>();
    }
} 