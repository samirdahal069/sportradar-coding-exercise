using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Services
{
    public class ApplictionDBContext : DbContext
    {

        public ApplictionDBContext(DbContextOptions<ApplictionDBContext> options) : base(options)
        {

        }

        public DbSet<Match> Matches { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("MatchDB");
        }
    }
}
