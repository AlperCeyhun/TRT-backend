namespace TRT_backend.Models.DTO
{
    public class ClaimLanguageDto
    {
        public int Id { get; set; }
        public int ClaimId { get; set; }
        public int LanguageId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ClaimName { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = string.Empty;
        public string LanguageName { get; set; } = string.Empty;
    }
} 