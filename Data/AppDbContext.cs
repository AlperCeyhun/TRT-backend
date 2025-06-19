using Microsoft.EntityFrameworkCore;
using TRT_backend.Models;

namespace TRT_backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TodoTask> Tasks { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Assignee> Assignees {get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TodoTask>()
                .Property(e => e.Category)
                .HasConversion<string>();
        }
    }
} 