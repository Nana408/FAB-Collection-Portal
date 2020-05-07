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

            string message;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.ElementAt(0);

                var messList = errors.Errors.ElementAt(0);

                message = messList.ErrorMessage;

                UserFunctions.UpdateLogs(logID, StaticVariables.FAILSTATUS, message);

                return View();
            }


            if (UserFunctions.Login(logID, loginRequest.Username, loginRequest.Password, out LoginObject loginDetails, out message))
            {
                Session["Branch"] = loginDetails.MESSAGE.UserData.UserBranch;

                Session["SourceName"] = string.Format("{0} {1}" ,loginDetails.MESSAGE.UserData.FirstName ,loginDetails.MESSAGE.UserData.LastName);

                Session["SourceID"] = loginDetails.MESSAGE.UserData.UserId;

                Session["Till"] = loginDetails.MESSAGE.UserData.PointingAccount;

                UserFunctions.UpdateLogs(logID, StaticVariables.SUCCESSSTATUS, message);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.NotValidUser = message;

                UserFunctions.UpdateLogs(logID, StaticVariables.FAILSTATUS, message);

                return View();
            }

        }

        public ActionResult LogOff()
        {

            Session["Branch"] = null;
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