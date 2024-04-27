using System;
namespace khasGraduationProject.Models
{
	public class Appointments
	{
		public int id { get; set; }
		public DateTime date { get; set;}
		public TimeSpan time { get; set; }
        public int patient_id { get; set; }
        public int doctor_id { get; set; }
        public bool isCancelled { get; set; }

    }

    public class AppointmentViewModel
    {
        public int AppointmentId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public bool IsCancelled { get; set; }
        public string DoctorName { get; set; }
        public string DoctorSurname { get; set; }
        public string DoctorProfileImgPath { get; set; }
        public string DoctorEmail { get; set; }
        public string DoctorPhone { get; set; }
        public string PatientName { get; set; }
        public string PatientSurname { get; set; }
        public string PatientProfileImgPath { get; set; }
        public DateTime PatientBirthday { get; set; }
        public string PatientEmail { get; set; }

    }
}

