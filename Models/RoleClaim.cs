using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TRT_backend.Models
{
    public class RoleClaim
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Role")]
        public int RoleId { get; set; }
        public Role Role { get; set; }
        [ForeignKey("Claim")]
        public int ClaimId { get; set; }
        public Claim Claim { get; set; }
    }
} 