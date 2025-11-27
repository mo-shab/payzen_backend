using Microsoft.EntityFrameworkCore;
using payzen_backend.Models.Users;
using payzen_backend.Models;
using payzen_backend.Models.Permissions;
using payzen_backend.Models.Employee;
using payzen_backend.Models.Company;

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
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Company> Companies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration Employee
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("Employee");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.FirstName).HasColumnName("first_name").HasMaxLength(500).IsRequired();
                entity.Property(e => e.LastName).HasColumnName("last_name").HasMaxLength(500).IsRequired();
                entity.Property(e => e.CinNumber).HasColumnName("cin_number").HasMaxLength(500).IsRequired();
                entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth").IsRequired();
                entity.Property(e => e.Phone).HasColumnName("phone").IsRequired();
                entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(500).IsRequired();
                entity.Property(e => e.CompanyId).HasColumnName("company_id").IsRequired();
                entity.Property(e => e.ManagerId).HasColumnName("manager_id");
                entity.Property(e => e.StatusId).HasColumnName("status_id");
                entity.Property(e => e.GenderId).HasColumnName("gender_id");
                entity.Property(e => e.NationalityId).HasColumnName("nationality_id");
                entity.Property(e => e.EducationLevelId).HasColumnName("education_level_id");
                entity.Property(e => e.MaritalStatusId).HasColumnName("marital_status_id");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.CreatedBy).HasColumnName("created_by");
                entity.Property(e => e.ModifiedAt).HasColumnName("modified_at");
                entity.Property(e => e.ModifiedBy).HasColumnName("modified_by");
                entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
                entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");

                // Relations
                entity.HasOne(e => e.Company)
                    .WithMany(c => c.Employees)
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Manager)
                    .WithMany(m => m.Subordinates)
                    .HasForeignKey(e => e.ManagerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuration Company
            modelBuilder.Entity<Company>(entity =>
            {
                entity.ToTable("Company");
                entity.HasKey(c => c.Id);
                
                entity.Property(c => c.CompanyName).HasColumnName("company_name").HasMaxLength(500).IsRequired();
                entity.Property(c => c.CompanyAddress).HasColumnName("company_address").HasMaxLength(500).IsRequired();
                entity.Property(c => c.CityId).HasColumnName("city_id");
                entity.Property(c => c.CountryId).HasColumnName("country_id");
                entity.Property(c => c.IceNumber).HasColumnName("ice_number").HasMaxLength(500).IsRequired();
                entity.Property(c => c.CnssNumber).HasColumnName("cnss_number").HasMaxLength(500).IsRequired();
                entity.Property(c => c.IfNumber).HasColumnName("if_number").HasMaxLength(500).IsRequired();
                entity.Property(c => c.RcNumber).HasColumnName("rc_number").HasMaxLength(500).IsRequired();
                entity.Property(c => c.RibNumber).HasColumnName("rib_number").HasMaxLength(500).IsRequired();
                entity.Property(c => c.PhoneNumber).HasColumnName("phone_number").IsRequired();
                entity.Property(c => c.Email).HasColumnName("email").HasMaxLength(500).IsRequired();
                entity.Property(c => c.ManagedByCompanyId).HasColumnName("managedby_company_id");
                entity.Property(c => c.IsCabinetExpert).HasColumnName("is_cabinet_expert").HasDefaultValue(false);
                entity.Property(c => c.CreatedAt).HasColumnName("created_at");
                entity.Property(c => c.CreatedBy).HasColumnName("created_by");
                entity.Property(c => c.ModifiedAt).HasColumnName("modified_at");
                entity.Property(c => c.ModifiedBy).HasColumnName("modified_by");
                entity.Property(c => c.DeletedAt).HasColumnName("deleted_at");
                entity.Property(c => c.DeletedBy).HasColumnName("deleted_by");

                // Relation hiérarchique
                entity.HasOne(c => c.ManagedByCompany)
                    .WithMany(c => c.ManagedCompanies)
                    .HasForeignKey(c => c.ManagedByCompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Configuration User -> Employee
            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasOne(u => u.Employee)
                    .WithMany()
                    .HasForeignKey(u => u.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

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
