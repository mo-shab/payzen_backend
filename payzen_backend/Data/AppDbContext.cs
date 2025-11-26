using Microsoft.EntityFrameworkCore;
using payzen_backend.Models.Users;
using payzen_backend.Models;
using payzen_backend.Models.Permissions;

namespace payzen_backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Users> Users { get; set; }
        public DbSet<Permissions> Permissions { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<RolesPermissions> RolesPermissions { get; set; }
        public DbSet<UsersRoles> UsersRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Users entity
            modelBuilder.Entity<Users>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<Users>()
                .HasIndex(u => u.Email)
                .IsUnique(false);
            
            modelBuilder.Entity<Users>()
                .HasIndex(u => u.Username)
                .IsUnique(true);

            modelBuilder.Entity<Users>()
                .Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Users>()
                .Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            // Configuration pour Permissions
            modelBuilder.Entity<Permissions>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Permissions>()
                .HasIndex(p => p.Name)
                .IsUnique(true)
                .HasFilter("[DeletedAt] IS NULL");

            modelBuilder.Entity<Permissions>()
                .Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Permissions>()
                .Property(p => p.Description)
                .IsRequired()
                .HasMaxLength(500);

            // Configuration pour Roles
            modelBuilder.Entity<Roles>()
                .HasKey(r => r.Id);

            modelBuilder.Entity<Roles>()
                .HasIndex(r => r.Name)
                .IsUnique(true)
                .HasFilter("[DeletedAt] IS NULL");

            modelBuilder.Entity<Roles>()
                .Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Roles>()
                .Property(r => r.Description)
                .IsRequired()
                .HasMaxLength(500);

            modelBuilder.Entity<RolesPermissions>()
                .HasKey(rp => rp.Id);

            modelBuilder.Entity<RolesPermissions>()
                .HasIndex(rp => new { rp.RoleId, rp.PermissionId })
                .IsUnique(true)
                .HasFilter("[DeletedAt] IS NULL");

            // Relation avec Roles
            modelBuilder.Entity<RolesPermissions>()
                .HasOne(rp => rp.Role)
                .WithMany()
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relation avec Permissions
            modelBuilder.Entity<RolesPermissions>()
                .HasOne(rp => rp.Permission)
                .WithMany()
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuration pour UsersRoles (table de liaison)
            modelBuilder.Entity<UsersRoles>()
                .HasKey(ur => ur.Id);

            // Index unique pour éviter les doublons User-Role
            modelBuilder.Entity<UsersRoles>()
                .HasIndex(ur => new { ur.UserId, ur.RoleId })
                .IsUnique(true)
                .HasFilter("[DeletedAt] IS NULL");

            // Relation avec Users
            modelBuilder.Entity<UsersRoles>()
                .HasOne(ur => ur.User)
                .WithMany()
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relation avec Roles
            modelBuilder.Entity<UsersRoles>()
                .HasOne(ur => ur.Role)
                .WithMany()
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index unique composite filtré
            modelBuilder.Entity<RolesPermissions>()
                .HasIndex(rp => new { rp.RoleId, rp.PermissionId })
                .IsUnique(true)
                .HasFilter("[DeletedAt] IS NULL");
        }
    }
}
