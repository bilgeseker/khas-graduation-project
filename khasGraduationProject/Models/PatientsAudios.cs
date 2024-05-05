using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace khasGraduationProject.Models
{
	public class PatientsAudios
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
		public int app_id { get; set;}
		public int audio_id { get; set; }
        public string patientAudioFilePath { get; set; }
        public string patientAudioText { get; set; }
        public double percentage { get; set; }        
    }

    public class AudioPatientViewModel
    {
        public int AudioId { get; set; }
        public string AudioFilePath { get; set; }
        public string AudioText { get; set; }
        public int PatientsAudiosId { get; set; }
        public int PatientsAudiosAudioId { get; set; }
        public int PatientsAudiosAppId { get; set; }
        public string PatientAudioFilePath { get; set; }
        public string PatientAudioText { get; set; }
        public double PatientAudioPercentage { get; set; }
    }
}

