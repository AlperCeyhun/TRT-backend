using System;
using System.ComponentModel.DataAnnotations;

namespace TRT_backend.Models
{
    public class AppLanguage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } // "Türkçe", "English" 

        [Required]
        public string LanguageId { get; set; } // "tr-TR", "en-US" .

        public bool IsActive { get; set; } = true;

        
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

       
    }
} 