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
        [ForeignKey("Language")]
        public int LanguageId { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public Claims Claim { get; set; }
        public Language Language { get; set; }
    }
}
