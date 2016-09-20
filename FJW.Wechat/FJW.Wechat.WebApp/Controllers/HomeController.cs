﻿using FJW.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FJW.Wechat.WebApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return Content(Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery,""));
        }
    }
}