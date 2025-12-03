using Microsoft.EntityFrameworkCore;
using payzen_backend.Models.Users;
using payzen_backend.Models;
using payzen_backend.Models.Permissions;
using payzen_backend.Models.Employee;
using payzen_backend.Models.Company;
using payzen_backend.Models.Referentiel;

namespace payzen_backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ========== Tables Users & Permissions ==========
        public DbSet<Users> Users { get; set; }
        public DbSet<Permissions> Permissions { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<RolesPermissions> RolesPermissions { get; set; }
        public DbSet<UsersRoles> UsersRoles { get; set; }

        // ========== Tables Company ==========
        public DbSet<Company> Companies { get; set; }
        public DbSet<Departement> Departement { get; set; }
        public DbSet<ContractType> ContractTypes { get; set; }
        public DbSet<JobPosition> JobPositions { get; set; }
        public DbSet<Holiday> Holidays { get; set; }
        public DbSet<WorkingCalendar> WorkingCalendars { get; set; }

        // ========== Tables Referentiel ==========
        public DbSet<Country> Countries { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<Gender> Genders { get; set; }
        public DbSet<EducationLevel> EducationLevels { get; set; }
        public DbSet<MaritalStatus> MaritalStatuses { get; set; }

        // ========== Tables Employee ==========
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeContract> EmployeeContracts { get; set; }
        public DbSet<EmployeeSalary> EmployeeSalaries { get; set; }
        public DbSet<EmployeeSalaryComponent> EmployeeSalaryComponents { get; set; }
        public DbSet<EmployeeAddress> EmployeeAddresses { get; set; }
        public DbSet<EmployeeDocument> EmployeeDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Users>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique(false);
                entity.HasIndex(u => u.Username).IsUnique(true).HasFilter("[DeletedAt] IS NULL");
                
                entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);

                entity.HasOne(u => u.Employee)
                    .WithMany()
                    .HasForeignKey(u => u.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Permissions>(entity =>
            {
                entity.ToTable("Permissions");
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.Name).IsUnique(true).HasFilter("[DeletedAt] IS NULL");
                
                entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Description).IsRequired().HasMaxLength(500);
            });

            modelBuilder.Entity<Roles>(entity =>
            {
                entity.ToTable("Roles");
                entity.HasKey(r => r.Id);
                entity.HasIndex(r => r.Name).IsUnique(true).HasFilter("[DeletedAt] IS NULL");
                
                entity.Property(r => r.Name).IsRequired().HasMaxLength(50);
                entity.Property(r => r.Description).IsRequired().HasMaxLength(500);
            });

            modelBuilder.Entity<RolesPermissions>(entity =>
            {
                entity.ToTable("RolesPermissions");
                entity.HasKey(rp => rp.Id);
                entity.HasIndex(rp => new { rp.RoleId, rp.PermissionId }).IsUnique(true).HasFilter("[DeletedAt] IS NULL");

                entity.HasOne(rp => rp.Role)
                    .WithMany()
                    .HasForeignKey(rp => rp.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rp => rp.Permission)
                    .WithMany()
                    .HasForeignKey(rp => rp.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UsersRoles>(entity =>
            {
                entity.ToTable("UsersRoles");
                entity.HasKey(ur => ur.Id);
                entity.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique(true).HasFilter("[DeletedAt] IS NULL");

                entity.HasOne(ur => ur.User)
                    .WithMany()
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ur => ur.Role)
                    .WithMany()
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ========================================
            // CONFIGURATION REFERENTIEL
            // ========================================

            modelBuilder.Entity<Country>(entity =>
            {
                entity.ToTable("Country");
                entity.HasKey(c => c.Id);
                entity.HasIndex(c => c.CountryCode).IsUnique().HasFilter("[DeletedAt] IS NULL");
                
                entity.Property(c => c.CountryName).IsRequired().HasMaxLength(500);
                entity.Property(c => c.CountryNameAr).HasMaxLength(500);
                entity.Property(c => c.CountryCode).IsRequired().HasMaxLength(3);
                entity.Property(c => c.CountryPhoneCode).IsRequired().HasMaxLength(10);
                entity.Property(c => c.Nationality).IsRequired().HasMaxLength(500);
            });

            modelBuilder.Entity<City>(entity =>
            {
                entity.ToTable("City");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.CityName).IsRequired().HasMaxLength(500);

                entity.HasOne(c => c.Country)
                    .WithMany(co => co.Cities)
                    .HasForeignKey(c => c.CountryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Status>(entity =>
            {
                entity.ToTable("Status");
                entity.HasKey(s => s.Id);
                entity.HasIndex(s => s.Name).IsUnique().HasFilter("[DeletedAt] IS NULL");
                entity.Property(s => s.Name).IsRequired().HasMaxLength(100);
            });

            modelBuilder.Entity<Gender>(entity =>
            {
                entity.ToTable("Gender");
                entity.HasKey(g => g.Id);
                entity.HasIndex(g => g.Name).IsUnique().HasFilter("[DeletedAt] IS NULL");
                entity.Property(g => g.Name).IsRequired().HasMaxLength(50);
            });

            modelBuilder.Entity<EducationLevel>(entity =>
            {
                entity.ToTable("EducationLevel");
                entity.HasKey(el => el.Id);
                entity.HasIndex(el => el.Name).IsUnique().HasFilter("[DeletedAt] IS NULL");
                entity.Property(el => el.Name).IsRequired().HasMaxLength(100);
            });

            modelBuilder.Entity<MaritalStatus>(entity =>
            {
                entity.ToTable("MaritalStatus");
                entity.HasKey(ms => ms.Id);
                entity.HasIndex(ms => ms.Name).IsUnique().HasFilter("[DeletedAt] IS NULL");
                entity.Property(ms => ms.Name).IsRequired().HasMaxLength(50);
            });

            // ========================================
            // CONFIGURATION COMPANY
            // ========================================

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

                entity.HasOne(c => c.ManagedByCompany)
                    .WithMany(c => c.ManagedCompanies)
                    .HasForeignKey(c => c.ManagedByCompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.City)
                    .WithMany(ci => ci.Companies)
                    .HasForeignKey(c => c.CityId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Country)
                    .WithMany(co => co.Companies)
                    .HasForeignKey(c => c.CountryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Departement>(entity =>
            {
                entity.ToTable("Departement");
                entity.HasKey(d => d.Id);
                entity.Property(d => d.DepartementName).IsRequired().HasMaxLength(100);

                entity.HasOne(d => d.Company)
                    .WithMany()
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ContractType>(entity =>
            {
                entity.ToTable("ContractType");
                entity.HasKey(ct => ct.Id);
                entity.Property(ct => ct.ContractTypeName).IsRequired().HasMaxLength(100);

                entity.HasOne(ct => ct.Company)
                    .WithMany()
                    .HasForeignKey(ct => ct.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<JobPosition>(entity =>
            {
                entity.ToTable("JobPosition");
                entity.HasKey(jp => jp.Id);
                entity.Property(jp => jp.Name).IsRequired().HasMaxLength(200);

                entity.HasOne(jp => jp.Company)
                    .WithMany()
                    .HasForeignKey(jp => jp.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Holiday>(entity =>
            {
                entity.ToTable("Holiday");
                entity.HasKey(h => h.Id);
                entity.Property(h => h.Name).IsRequired().HasMaxLength(500);
                entity.Property(h => h.HolidayDate).HasColumnType("date");
                entity.HasIndex(h => new { h.CompanyId, h.HolidayDate }).IsUnique().HasFilter("[DeletedAt] IS NULL");

                entity.HasOne(h => h.Company)
                    .WithMany()
                    .HasForeignKey(h => h.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(h => h.Country)
                    .WithMany(c => c.Holidays)
                    .HasForeignKey(h => h.CountryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<WorkingCalendar>(entity =>
            {
                entity.ToTable("WorkingCalendar");
                entity.HasKey(wc => wc.Id);
                entity.Property(wc => wc.StartTime).HasColumnType("time");
                entity.Property(wc => wc.EndTime).HasColumnType("time");
                entity.HasIndex(wc => new { wc.CompanyId, wc.DayOfWeek }).IsUnique().HasFilter("[DeletedAt] IS NULL");

                entity.HasOne(wc => wc.Company)
                    .WithMany()
                    .HasForeignKey(wc => wc.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ========================================
            // CONFIGURATION EMPLOYEE
            // ========================================

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
                entity.Property(e => e.DepartementId).HasColumnName("departement_id");
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

                entity.HasIndex(e => e.CinNumber).IsUnique().HasFilter("[deleted_at] IS NULL");
                entity.HasIndex(e => e.Email).IsUnique().HasFilter("[deleted_at] IS NULL");

                entity.HasOne(e => e.Company)
                    .WithMany(c => c.Employees)
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Manager)
                    .WithMany(m => m.Subordinates)
                    .HasForeignKey(e => e.ManagerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Departement)
                    .WithMany(d => d.Employees)
                    .HasForeignKey(e => e.DepartementId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Status)
                    .WithMany(s => s.Employees)
                    .HasForeignKey(e => e.StatusId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Gender)
                    .WithMany(g => g.Employees)
                    .HasForeignKey(e => e.GenderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Nationality)
                    .WithMany()
                    .HasForeignKey(e => e.NationalityId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.EducationLevel)
                    .WithMany(el => el.Employees)
                    .HasForeignKey(e => e.EducationLevelId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.MaritalStatus)
                    .WithMany(ms => ms.Employees)
                    .HasForeignKey(e => e.MaritalStatusId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<EmployeeContract>(entity =>
            {
                entity.ToTable("EmployeeContract");
                entity.HasKey(ec => ec.Id);
                entity.Property(ec => ec.StartDate).IsRequired();

                entity.HasOne(ec => ec.Employee)
                    .WithMany(e => e.Contracts)
                    .HasForeignKey(ec => ec.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ec => ec.Company)
                    .WithMany()
                    .HasForeignKey(ec => ec.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ec => ec.JobPosition)
                    .WithMany(jp => jp.EmployeeContracts)
                    .HasForeignKey(ec => ec.JobPositionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ec => ec.ContractType)
                    .WithMany(ct => ct.Employees)
                    .HasForeignKey(ec => ec.ContractTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<EmployeeSalary>(entity =>
            {
                entity.ToTable("EmployeeSalary");
                entity.HasKey(es => es.Id);
                entity.Property(es => es.BaseSalary).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(es => es.EffectiveDate).IsRequired();

                entity.HasOne(es => es.Employee)
                    .WithMany(e => e.Salaries)
                    .HasForeignKey(es => es.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(es => es.Contract)
                    .WithMany(ec => ec.Salaries)
                    .HasForeignKey(es => es.ContractId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<EmployeeSalaryComponent>(entity =>
            {
                entity.ToTable("EmployeeSalaryComponent");
                entity.HasKey(esc => esc.Id);
                entity.Property(esc => esc.ComponentType).IsRequired().HasMaxLength(100);
                entity.Property(esc => esc.Amount).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(esc => esc.EffectiveDate).IsRequired();

                entity.HasOne(esc => esc.EmployeeSalary)
                    .WithMany(es => es.Components)
                    .HasForeignKey(esc => esc.EmployeeSalaryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<EmployeeAddress>(entity =>
            {
                entity.ToTable("EmployeeAddress");
                entity.HasKey(ea => ea.Id);
                entity.Property(ea => ea.AddressLine1).IsRequired().HasMaxLength(500);
                entity.Property(ea => ea.AddressLine2).HasMaxLength(500);
                entity.Property(ea => ea.ZipCode).IsRequired().HasMaxLength(20);

                entity.HasOne(ea => ea.Employee)
                    .WithMany(e => e.Addresses)
                    .HasForeignKey(ea => ea.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ea => ea.City)
                    .WithMany()
                    .HasForeignKey(ea => ea.CityId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ea => ea.Country)
                    .WithMany()
                    .HasForeignKey(ea => ea.CountryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<EmployeeDocument>(entity =>
            {
                entity.ToTable("EmployeeDocument");
                entity.HasKey(ed => ed.Id);
                entity.Property(ed => ed.Name).IsRequired().HasMaxLength(500);
                entity.Property(ed => ed.FilePath).IsRequired().HasMaxLength(1000);
                entity.Property(ed => ed.DocumentType).IsRequired().HasMaxLength(100);

                entity.HasOne(ed => ed.Employee)
                    .WithMany(e => e.Documents)
                    .HasForeignKey(ed => ed.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
