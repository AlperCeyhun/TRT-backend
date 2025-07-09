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
        public DbSet<Language> Languages { get; set; }
        public DbSet<ClaimLanguage> ClaimLanguages { get; set; }
        public DbSet<TaskCategory> TaskCategories { get; set; }
        

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

            // TaskCategory ile TodoTask ilişkisi
            modelBuilder.Entity<TodoTask>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Tasks)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Claims>().HasData(
                new Claims { Id = 1, ClaimName = "AddTask" },
                new Claims { Id = 2, ClaimName = "DeleteTask" },
                new Claims { Id = 3, ClaimName = "EditTaskTitle" },
                new Claims { Id = 4, ClaimName = "EditTaskDescription" },
                new Claims { Id = 5, ClaimName = "EditTaskStatus" },
                new Claims { Id = 6, ClaimName = "EditTaskAssignees" }
            );

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, RoleName = "Admin" },
                new Role { Id = 2, RoleName = "User" }
            );

            modelBuilder.Entity<RoleClaim>().HasData(
                new RoleClaim { Id = 1, RoleId = 1, ClaimId = 1 }, // Add Task
                new RoleClaim { Id = 2, RoleId = 1, ClaimId = 2 }, // Delete Task
                new RoleClaim { Id = 3, RoleId = 1, ClaimId = 3 }, // Edit Task Title
                new RoleClaim { Id = 4, RoleId = 1, ClaimId = 4 }, // Edit Task Description
                new RoleClaim { Id = 5, RoleId = 1, ClaimId = 5 }, // Edit Task Status
                new RoleClaim { Id = 6, RoleId = 1, ClaimId = 6 }  // Edit Task Assignees
            );

            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, username = "admin", password = "admin123" }
            );

            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { Id = 1, UserId = 1, RoleId = 1 }
            );

            modelBuilder.Entity<TaskCategory>().HasData(
                new TaskCategory { Id = 1, Name = "Genel", Description = "Genel görevler", Color = "#007bff", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new TaskCategory { Id = 2, Name = "Acil", Description = "Acil görevler", Color = "#dc3545", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new TaskCategory { Id = 3, Name = "Önemli", Description = "Önemli görevler", Color = "#ffc107", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new TaskCategory { Id = 4, Name = "Düşük Öncelik", Description = "Düşük öncelikli görevler", Color = "#6c757d", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            modelBuilder.Entity<Language>().HasData(
                new Language { Id = 1, Code = "tr", Name = "Türkçe" },
                new Language { Id = 2, Code = "en", Name = "English" },
                new Language { Id = 3, Code = "fr", Name = "Français" }
            );

            modelBuilder.Entity<ClaimLanguage>().HasData(
                // Türkçe çeviriler
                new ClaimLanguage { Id = 1, ClaimId = 1, LanguageId = 1, Name = "GörevEkle", Description = "Yeni görev oluşturma izni" },
                new ClaimLanguage { Id = 2, ClaimId = 2, LanguageId = 1, Name = "GörevSil", Description = "Görev silme izni" },
                new ClaimLanguage { Id = 3, ClaimId = 3, LanguageId = 1, Name = "GörevBaşlığınıDüzenle", Description = "Görev başlığını değiştirme izni" },
                new ClaimLanguage { Id = 4, ClaimId = 4, LanguageId = 1, Name = "GörevAçıklamasınıDüzenle", Description = "Görev açıklamasını değiştirme izni" },
                new ClaimLanguage { Id = 5, ClaimId = 5, LanguageId = 1, Name = "GörevDurumunuDüzenle", Description = "Görev durumunu değiştirme izni" },
                new ClaimLanguage { Id = 6, ClaimId = 6, LanguageId = 1, Name = "GörevAtayanlarıDüzenle", Description = "Görev atayanlarını değiştirme izni" },
                
                // İngilizce çeviriler
                new ClaimLanguage { Id = 7, ClaimId = 1, LanguageId = 2, Name = "AddTask", Description = "Permission to create new task" },
                new ClaimLanguage { Id = 8, ClaimId = 2, LanguageId = 2, Name = "DeleteTask", Description = "Permission to delete task" },
                new ClaimLanguage { Id = 9, ClaimId = 3, LanguageId = 2, Name = "EditTaskTitle", Description = "Permission to edit task title" },
                new ClaimLanguage { Id = 10, ClaimId = 4, LanguageId = 2, Name = "EditTaskDescription", Description = "Permission to edit task description" },
                new ClaimLanguage { Id = 11, ClaimId = 5, LanguageId = 2, Name = "EditTaskStatus", Description = "Permission to edit task status" },
                new ClaimLanguage { Id = 12, ClaimId = 6, LanguageId = 2, Name = "EditTaskAssignees", Description = "Permission to edit task assignees" },
                
                // Fransızca çeviriler
                new ClaimLanguage { Id = 13, ClaimId = 1, LanguageId = 3, Name = "AjouterTâche", Description = "Permission d'ajouter une tâche" },
                new ClaimLanguage { Id = 14, ClaimId = 2, LanguageId = 3, Name = "SupprimerTâche", Description = "Permission de supprimer une tâche" },
                new ClaimLanguage { Id = 15, ClaimId = 3, LanguageId = 3, Name = "ModifierTitreTâche", Description = "Permission de modifier le titre de la tâche" },
                new ClaimLanguage { Id = 16, ClaimId = 4, LanguageId = 3, Name = "ModifierDescriptionTâche", Description = "Permission de modifier la description de la tâche" },
                new ClaimLanguage { Id = 17, ClaimId = 5, LanguageId = 3, Name = "ModifierStatutTâche", Description = "Permission de modifier le statut de la tâche" },
                new ClaimLanguage { Id = 18, ClaimId = 6, LanguageId = 3, Name = "ModifierAssignésTâche", Description = "Permission de modifier les assignés de la tâche" }
            );
        
        }
    }
} 