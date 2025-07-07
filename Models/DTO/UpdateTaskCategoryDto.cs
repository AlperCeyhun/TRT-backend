using System.ComponentModel.DataAnnotations;

namespace TRT_backend.Models.DTO
{
    public class UpdateTaskCategoryDto
    {
        [StringLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olabilir")]
        public string? Name { get; set; }
        
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        public string? Description { get; set; }
        
        public string? Color { get; set; }
    }
} 