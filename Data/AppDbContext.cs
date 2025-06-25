using Microsoft.EntityFrameworkCore;
using TRT_backend.Models;

namespace TRT_backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Message> Messages { get; set; }

        public DbSet<TodoTask> Tasks { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Assignee> Assignees {get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Claims> Claims { get; set; }
        public DbSet<UserClaim> UserClaims { get; set; }
        public DbSet<RoleClaim> RoleClaims { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Message>()
    .HasOne(m => m.FromUser)
    .WithMany()
    .HasForeignKey(m => m.FromUserId)
    .OnDelete(DeleteBehavior.Restrict);

modelBuilder.Entity<Message>()
    .HasOne(m => m.ToUser)
    .WithMany()
    .HasForeignKey(m => m.ToUserId)
    .OnDelete(DeleteBehavior.Restrict); 

    
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

            // UserRole ilişkileri
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            // UserClaim ilişkileri
            modelBuilder.Entity<UserClaim>()
                .HasOne(uc => uc.User)
                .WithMany(u => u.UserClaims)
                .HasForeignKey(uc => uc.UserId);
            modelBuilder.Entity<UserClaim>()
                .HasOne(uc => uc.Claim)
                .WithMany(c => c.UserClaims)
                .HasForeignKey(uc => uc.ClaimId);

            // RoleClaim ilişkileri
            modelBuilder.Entity<RoleClaim>()
                .HasOne(rc => rc.Role)
                .WithMany(r => r.RoleClaims)
                .HasForeignKey(rc => rc.RoleId);
            modelBuilder.Entity<RoleClaim>()
                .HasOne(rc => rc.Claim)
                .WithMany(c => c.RoleClaims)
                .HasForeignKey(rc => rc.ClaimId);

            // Message ilişkileri
            modelBuilder.Entity<Message>()
                .HasOne(m => m.FromUser)
                .WithMany()
                .HasForeignKey(m => m.FromUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.ToUser)
                .WithMany()
                .HasForeignKey(m => m.ToUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Claims>().HasData(
                new Claims { Id = 1, ClaimName = "Add Task" },
                new Claims { Id = 2, ClaimName = "Delete User" },
                new Claims { Id = 3, ClaimName = "Edit Task Title" },
                new Claims { Id = 4, ClaimName = "Edit Task Description" },
                new Claims { Id = 5, ClaimName = "Edit Task Status" },
                new Claims { Id = 6, ClaimName = "Edit Task Assignees" },
                new Claims { Id = 7, ClaimName = "Add Claim to User" },
                new Claims { Id = 8, ClaimName = "Delete Task" }
            );

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, RoleName = "Admin" },
                new Role { Id = 2, RoleName = "User" }
            );

            modelBuilder.Entity<RoleClaim>().HasData(
                new RoleClaim { Id = 1, RoleId = 1, ClaimId = 1 }, // Add Task
                new RoleClaim { Id = 2, RoleId = 1, ClaimId = 2 }, // Delete User
                new RoleClaim { Id = 3, RoleId = 1, ClaimId = 3 }, // Edit Task Title
                new RoleClaim { Id = 4, RoleId = 1, ClaimId = 4 }, // Edit Task Description
                new RoleClaim { Id = 5, RoleId = 1, ClaimId = 5 }, // Edit Task Status
                new RoleClaim { Id = 6, RoleId = 1, ClaimId = 6 }, // Edit Task Assignees
                new RoleClaim { Id = 7, RoleId = 1, ClaimId = 7 }, // Add Claim to User
                new RoleClaim { Id = 8, RoleId = 1, ClaimId = 8 }  // Delete Task
            );

        
        }
    }
} 