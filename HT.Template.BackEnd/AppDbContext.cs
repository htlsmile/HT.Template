using HT.Template.BackEnd.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace HT.Template.BackEnd
{
    public class AppDbContext : IdentityDbContext<
        User, Role, Guid,
        UserClaim, UserRole, UserLogin,
        RoleClaim, UserToken>
    {
        public AppDbContext() : base(Config.DbContextOptions) { }

        public AppDbContext(DbContextOptions options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.Configure();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Configure();
        }
    }
}
