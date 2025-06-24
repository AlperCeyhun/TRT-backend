using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TRT_backend.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string RoleName { get; set; } // "Admin", "User"
        public ICollection<UserRole> UserRoles { get; set; }
        public ICollection<RoleClaim> RoleClaims { get; set; }
    }
} 