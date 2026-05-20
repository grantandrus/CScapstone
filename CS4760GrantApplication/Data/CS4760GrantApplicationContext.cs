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
        public DbSet<Department> Departments { get; set; } = default!;
        public DbSet<Grant> Grants { get; set; } = default!;
    }
}
