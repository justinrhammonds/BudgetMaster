using AspNetIdentity2.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BudgetMaster.Controllers
{
    [RequireHttps]
    public class HomeController : ApplicationBaseController
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}