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

            // Assignee ile User ilişkisi
            modelBuilder.Entity<Assignee>()
                .HasOne(a => a.User)
                .WithMany(u => u.Assignees)
                .HasForeignKey(a => a.UserId);

            // Assignee ile Task ilişkisi
            modelBuilder.Entity<Assignee>()
                .HasOne(a => a.Task)
                .WithMany(t => t.Assignees)
                .HasForeignKey(a => a.TaskId);
        }
    }
} 