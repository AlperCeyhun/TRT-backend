using System.ComponentModel.DataAnnotations;

namespace TRT_backend.Models
{
    public class TodoTask
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "It can't be empty.")]
        public string Title { get; set; }

        public string Description { get; set; }
        public TaskCategory Category { get; set; }

        [Required(ErrorMessage = "It can't be empty.")]
        public bool Completed { get; set; }
    }

    public enum TaskCategory
    {
        Acil,
        Normal,
        DusukOncelik
    }
} 