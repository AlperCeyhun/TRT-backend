using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace TRT_backend.Models
{
    public class TodoTask
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "It can't be empty.")]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "It can't be empty.")]
        public bool Completed { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Foreign key for TaskCategory
        public int? CategoryId { get; set; }

        // Navigation properties
        public virtual TaskCategory? Category { get; set; }
        public virtual ICollection<Assignee> Assignees { get; set; } = new List<Assignee>();
    }
} 