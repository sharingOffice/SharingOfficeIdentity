using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SharingOffice.Domain.models;
using BC = BCrypt.Net.BCrypt;


namespace SharingOffice.Infra.DbContexts
{
    public class SharringOfficeDbContext: IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        //Aggregate
        public override DbSet<User> Users { get; set; }
        
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public override DbSet<IdentityUserClaim<Guid>> UserClaims { get; set; }

        public override DbSet<IdentityUserLogin<Guid>> UserLogins { get; set; }

        public override DbSet<IdentityUserRole<Guid>> UserRoles { get; set; }

        public override DbSet<IdentityUserToken<Guid>> UserTokens { get; set; }

        public override DbSet<IdentityRole<Guid>> Roles { get; set; }

        public override DbSet<IdentityRoleClaim<Guid>> RoleClaims { get; set; }

        public SharringOfficeDbContext(DbContextOptions<SharringOfficeDbContext> options) : base(options)
        {
            
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    "Data Source=localhost,1433;Initial Catalog=SharingOfficeidentityDb;Persist Security Info=True;User ID=sa;Password=YYyy12!@;Max Pool Size=80");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasKey(q => q.Id);
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<User>().Ignore(q => q.FullName);
            modelBuilder.Entity<User>().HasData(new List<User>()
            {
                new User()
                {
                    Id = Guid.NewGuid(),
                    UserName = "unos.bm65@gmail.com",
                    PasswordHash = BC.HashPassword("YYyy12!@"),
                    Email = "unos.bm65@gmail.com",
                    FirstName = "Younes",
                    LastName = "Baghaei",
                    EmailConfirmed = true,
                    NormalizedEmail = "UNOS.BM65@GMAIL.COM",
                    PhoneNumber = "123456789",
                    PhoneNumberConfirmed = true,
                    NormalizedUserName = "UNOS.BM65@GMAIL.COM",
                    TwoFactorEnabled = false
                }
            });

            modelBuilder.Entity<RefreshToken>().HasKey(q => q.Id);
            modelBuilder.Entity<RefreshToken>().ToTable("RefreshTokens");
            modelBuilder.Entity<RefreshToken>().HasOne(q => q.User);
            
            modelBuilder.Entity<IdentityRole>().ToTable("Roles")
                .HasData(new List<IdentityRole>()
            {
                new IdentityRole("Guest"),
                new IdentityRole("User"),
                new IdentityRole("Admin"),
            });

            modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        }
    }
}