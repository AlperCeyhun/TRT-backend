using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TRT_backend.Models
{
    public class ClaimLanguage
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Claims")]
        public int ClaimId { get; set; }
        public Claims Claims { get; set; }

        [ForeignKey("AppLanguage")]
        public int AppLanguageId { get; set; }
        public AppLanguage AppLanguage { get; set; }

        [Required]
        public string Name { get; set; }

        public bool IsActive { get; set; } = true;

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
} 