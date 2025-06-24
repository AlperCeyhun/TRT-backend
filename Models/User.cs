using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace TRT_backend.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "It can't be empty.")]
        public string username { get; set; }
        [Required(ErrorMessage = "It can't be empty.")]
        public string password { get; set; }
        
        
        public ICollection<Assignee> Assignees { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
        public ICollection<UserClaim> UserClaims { get; set; }
    }
}
