﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetMedsFull.Controllers
{
    public class ProductController : Controller
    {
        
        public IActionResult Detail()
        {
            return View();
        }
        public IActionResult Shop()
        {
            return View();
        }
    }
}
