using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TRT_backend.Models
{
    public class UserClaim
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }
        [ForeignKey("Claims")]
        public int ClaimId { get; set; }
        public Claims Claim { get; set; }
    }
} 