using Microsoft.EntityFrameworkCore;
using System;
using DataAccess.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.EntityFrameworkCore.Design;

namespace DataAccess
{
    public class KlepaDbContext :  DbContext
    {
        public KlepaDbContext(DbContextOptions<KlepaDbContext> options) : base(options) {
           
        }

        public DbSet<Server> Servers { get; set; }
    }

    public class KlepaDbContextFactory : IDesignTimeDbContextFactory<KlepaDbContext>
    {
        public KlepaDbContext CreateDbContext(string[] args)
        {
            var optsBuilder = new DbContextOptionsBuilder<KlepaDbContext>();
            optsBuilder.UseSqlite("Data Source=Application.db");
            return new KlepaDbContext(optsBuilder.Options);
        }
    }
}
