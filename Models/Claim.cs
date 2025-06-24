using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TRT_backend.Models
{
    public class Claim
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string ClaimName { get; set; } // "Add Task", "Edit Task", vs.
        public ICollection<UserClaim> UserClaims { get; set; }
        public ICollection<RoleClaim> RoleClaims { get; set; }
    }
} 