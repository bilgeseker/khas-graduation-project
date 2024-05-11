﻿using System;
using System.ComponentModel.DataAnnotations;
namespace khasGraduationProject.Models
{
	public class AdminDetails
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public string email { get; set; }
        public string password { get; set; }
    }
}

