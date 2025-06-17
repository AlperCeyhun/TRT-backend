using Microsoft.EntityFrameworkCore;
using TRT_backend.Models;

namespace TRT_backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TodoTask> Tasks { get; set; }
    }
} 