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
        public int? CategoryId { get; set; }
        public Category Category { get; set; }

        [Required(ErrorMessage = "It can't be empty.")]
        public bool Completed { get; set; }

        
        public ICollection<Assignee> Assignees { get; set; }
    }
} 