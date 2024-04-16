using System;
namespace khasGraduationProject.Models
{
	public class DoctorDetails
	{
        public int id { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public int gender_id { get; set; }
        public int app_id { get; set; }
        public int states_id { get; set; }
        public int specialization_id { get; set; }
    }
}

