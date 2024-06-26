﻿using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using khasGraduationProject.Models;

namespace khasGraduationProject.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    { 
        _logger = logger;
    }

    public IActionResult Index()
    {
        var context = new WebContext();
        var patient = context.patients.ToList().Count();
        var doctor = context.doctors.ToList().Count();

        ViewBag.patientCount = patient;
        ViewBag.doctorCount = doctor;

        return View();
    }

    public IActionResult ContactUs()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

