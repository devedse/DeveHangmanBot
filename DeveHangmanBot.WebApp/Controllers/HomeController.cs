using DeveHangmanBot.WebApp.Logging;
using DeveHangmanBot.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;

namespace DeveHangmanBot.WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IConfiguration config, ILogger<HomeController> logger)
        {
            _config = config;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var configSettings = _config.GetChildren();
            var configSettingsListified = configSettings.Select(t => new SettingValueModel()
            {
                Key = t.Key,
                Value = t.Key.Contains("auth", StringComparison.OrdinalIgnoreCase) ||
                        t.Key.Contains("password", StringComparison.OrdinalIgnoreCase) ||
                        t.Key.Contains("token", StringComparison.OrdinalIgnoreCase) ||
                        t.Key.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
                        t.Key.Contains("key", StringComparison.OrdinalIgnoreCase)
                        ? new string('*', t.Value.Length) : t.Value
            }).ToList();
            return View(configSettingsListified);
        }

        public IActionResult Logging()
        {
            ViewData["Message"] = "Logging page.";

            return View(DirtyMemoryLogger.LoggingLines);
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
}
