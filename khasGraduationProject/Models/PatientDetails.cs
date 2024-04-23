using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace khasGraduationProject.Models
{
	public class PatientDetails
	{
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        public  string name { get; set; }
        public  string surname { get; set; }
        public DateTime birthday { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public int gender_id { get; set; }
        public int app_id { get; set; }
        public int doctor_id { get; set; }
        public int location_id { get; set; }
        public string profileImgPath { get; set; }


    }
}

