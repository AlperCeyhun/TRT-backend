using System.ComponentModel.DataAnnotations;

namespace TRT_backend.Models
{
    public class Language
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Code { get; set; } // "tr", "en", "fr"
        [Required]
        public string Name { get; set; } // "Türkçe", "English", "Français"
    }
} 