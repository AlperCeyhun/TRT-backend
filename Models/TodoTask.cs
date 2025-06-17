using System.ComponentModel.DataAnnotations;

namespace TRT_backend.Models
{
    public class TodoTask
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "It can't be empty.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "It can't be empty.")]
        public bool Status { get; set; }
    }
} 