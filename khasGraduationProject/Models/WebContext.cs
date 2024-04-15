using System;
using Microsoft.EntityFrameworkCore;

namespace khasGraduationProject.Models
{
	public class WebContext : DbContext
    {
		public DbSet<PatientDetails> patients { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Use Azure SQL Database provider
            optionsBuilder.UseSqlServer("Server=khasgraduationproject.database.windows.net;Database=fens402;User ID=ferihanbilge;Password=Khas2024*;Trusted_Connection=False;");
        }
    
	}
}

