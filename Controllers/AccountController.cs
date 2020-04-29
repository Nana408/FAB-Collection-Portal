using FAB_Merchant_Portal.Models;
using Newtonsoft.Json;
using System.Linq;
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
            var ipAddress = UserFunctions.GetIPAddress();

            int logID = UserFunctions.InsertLog(ipAddress, "Login", loginRequest.Username, loginRequest.Username, JsonConvert.SerializeObject(loginRequest.Username));
            string item = "User Does not Exists";
            item = "Success";
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.ElementAt(0);

                var messList = errors.Errors.ElementAt(0);

                item = messList.ErrorMessage;

                UserFunctions.UpdateLogs(logID, StaticVariables.FAILSTATUS, item);


                return View();
            }


            if (item == "Success")
            {
                Session["Branch"] = "Head Office";
                Session["SourceName"] = "John Amoah";
                Session["SourceID"] = "12345";
                Session["Till"] = "12345123332";

                UserFunctions.UpdateLogs(logID, StaticVariables.SUCCESSSTATUS, item);
                return RedirectToAction("Index", "Home");
            }
            else if (item == "User Does not Exists")
            {
                ViewBag.NotValidUser = item;

            }
            else
            {
                ViewBag.Failedcount = item;
            }
            UserFunctions.UpdateLogs(logID, StaticVariables.FAILSTATUS, item);

            return View();
        }

        public ActionResult LogOff()
        {
         
            Session["Branch"] =null;
            Session["SourceName"] = null;
            Session["SourceID"] = null;
            Session["Till"] = null;

            Session.Clear();
            Session.Abandon();
            FormsAuthentication.SignOut(); //you write this when you use FormsAuthentication


            return RedirectToAction("Login", "Account");
        }
    }
}