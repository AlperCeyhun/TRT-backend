using System.ComponentModel.DataAnnotations;

namespace TRT_backend.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "It can't be empty.")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "It can't be empty.")]
        public string Password { get; set; }
    }
}
