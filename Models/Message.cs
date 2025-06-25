using System.ComponentModel.DataAnnotations.Schema;
using TRT_backend.Models;

public class Message
{
    public int Id { get; set; }

    public string Content { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("FromUser")]
    public int FromUserId { get; set; }
    public User FromUser { get; set; }

    [ForeignKey("ToUser")]
    public int ToUserId { get; set; }
    public User ToUser { get; set; }
}