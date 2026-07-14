using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CS4760GrantApplication.Models;

namespace CS4760GrantApplication.Data
{
    public class CS4760GrantApplicationContext : DbContext
    {
        public CS4760GrantApplicationContext(DbContextOptions<CS4760GrantApplicationContext> options)
            : base(options)
        {
        }

        public DbSet<CS4760GrantApplication.Models.User> Users { get; set; } = default!;
        public DbSet<CS4760GrantApplication.Models.College> Colleges { get; set; } = default!;
        public DbSet<Department> Departments { get; set; } = default!;
        public DbSet<Grant> Grants { get; set; } = default!;
        public DbSet<GrantAttachment> GrantAttachments { get; set; } = default!;
        public DbSet<BudgetItem> BudgetItems { get; set; } = default!;
        public DbSet<Allocation> Allocations { get; set; } = default!;
        public DbSet<Review> Reveiws { get; set; } = default!;
        public DbSet<Notification> Notifications { get; set; } = default!;
        public DbSet<AllocationRule> AllocationRules { get; set; }
        public DbSet<Report> Reports { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Review>()
                .HasIndex(r => new { r.UserId, r.GrantId })
                .IsUnique();

            modelBuilder.Entity<College>()
                .HasOne(c => c.Dean)
                .WithMany()
                .HasForeignKey(c => c.DeanId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<User>()
                .HasOne(u => u.College)
                .WithMany()
                .HasForeignKey(u => u.CollegeId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Department)
                .WithMany()
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Department>()
                .HasOne(c => c.Chair)
                .WithMany()
                .HasForeignKey(c => c.ChairId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Department>()
                .HasOne(d => d.College)
                .WithMany()
                .HasForeignKey(d => d.CollegeId)
                .OnDelete(DeleteBehavior.SetNull);
        }
        public DbSet<RubricCriterion> RubricCriteria { get; set; } = default!;
        public DbSet<RatingSuggestion> RatingSuggestions { get; set; } = default!;
    }
}
