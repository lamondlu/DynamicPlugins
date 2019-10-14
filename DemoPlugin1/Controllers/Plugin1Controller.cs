﻿using Microsoft.AspNetCore.Mvc;

namespace DemoPlugin1.Controllers
{
    [Area("DemoPlugin1")]
    public class Plugin1Controller : Controller
    {
        public IActionResult HelloWorld() => View();

        public IActionResult Ping() => Content("pong");
    }
}
