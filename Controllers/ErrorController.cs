using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FAB_Merchant_Portal.Controllers
{
    public class ErrorController : Controller
    {
        [HttpGet]
        public ActionResult Oops(int id)
        {
            Response.StatusCode = id;
            ViewBag.StatusCode = id;
            return View();
        }

        public ViewResult Index()
        {
            return View("Error");
        }
        public ViewResult NotFound()
        {
            Response.StatusCode = 404;  //you may want to set this to 200
            return View("NotFound");
        }

        // GET:  internal server error
        public ActionResult InternalServerError()
        {
            Response.StatusCode = 500;
            return View("InternalServerError");
        }

        // GET:  page not found
        public ActionResult PageNotFound()
        {
            return View();
        }

        // GET:  unauthorized access
        public ActionResult UnAuthorized()
        {
            return View();
        }

        // GET:  forbiden page
        public ActionResult Forbiden()
        {
            return View();
        }
    }
}