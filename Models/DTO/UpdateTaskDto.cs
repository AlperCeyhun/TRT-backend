namespace TRT_backend.Models.DTO
{
    public class UpdateTaskDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Completed { get; set; }
        public int? CategoryId { get; set; }
    }
} 