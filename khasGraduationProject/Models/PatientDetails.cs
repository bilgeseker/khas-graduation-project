﻿using System;
namespace khasGraduationProject.Models
{
	public class PatientDetails
	{
        public int id { get; set; }
        public  string name { get; set; }
        public  string surname { get; set; }
        public DateTime birthday { get; set; }
        public int gender_id { get; set; }
        public int app_id { get; set; }
        public int doctor_id { get; set; }
        public int location_id { get; set; }
        
	}
}

