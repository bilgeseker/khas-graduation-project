using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace khasGraduationProject.Models
{
	public class Audios
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
		public string audioFilePath { get; set; }
        public string audioText { get; set; }
        public string similarText { get; set; }
    }
}

