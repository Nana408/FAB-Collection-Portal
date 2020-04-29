using FAB_Merchant_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace FAB_Merchant_Portal.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(LoginRequest loginRequest)
        {

            var item = "User Does not Exists";
            if (item == "Success")
            {

                return View("UserLandingView");
            }
            else if (item == "User Does not Exists")
            {
                ViewBag.NotValidUser = item;

            }
            else
            {
                ViewBag.Failedcount = item;
            }
            return View();
        }

        public ActionResult LogOff()
        {
            Session["Department"] = null;
            Session["UserName"] = null;
            Session.Clear();
            Session.Abandon();
            FormsAuthentication.SignOut(); //you write this when you use FormsAuthentication
            return RedirectToAction("Login", "Account");
        }
    }
}