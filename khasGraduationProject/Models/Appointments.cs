using System;
namespace khasGraduationProject.Models
{
	public class Appointments
	{
		public int id { get; set; }
		public string date { get; set;}
		public string time { get; set; }
        public int patient_id { get; set; }
        public int doctor_id { get; set; }
    }
}

