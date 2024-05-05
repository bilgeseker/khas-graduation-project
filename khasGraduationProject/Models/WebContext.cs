using System;
using Microsoft.EntityFrameworkCore;

namespace khasGraduationProject.Models
{
	public class WebContext : DbContext
    {
		public DbSet<PatientDetails> patients { get; set; }
        public DbSet<DoctorDetails> doctors { get; set; }
        public DbSet<States> states { get; set; }
        public DbSet<Specializations> specialization { get; set; }
        public DbSet<Genders> gender { get; set; }
        public DbSet<Appointments> appointments { get; set; }
        public DbSet<Audios> audios { get; set; }
        public DbSet<PatientsAudios> patientsAudios { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Use Azure SQL Database provider
            optionsBuilder.UseSqlServer("Server=khasgraduationproject.database.windows.net;Database=fens402;User ID=ferihanbilge;Password=Khas2024*;Trusted_Connection=False;Connection Timeout=60");
        }
    
	}
}

