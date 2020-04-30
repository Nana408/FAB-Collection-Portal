using FAB_Merchant_Portal.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc;

namespace FAB_Merchant_Portal.Controllers
{

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var ipAddress = UserFunctions.GetIPAddress();

            string sourceId = Session["SourceID"].ToString();
            string sourceName = Session["SourceName"].ToString();
            string branch = Session["Branch"].ToString();

            string requestBody = string.Empty;

            DateTime? startDate = null;

            DateTime? endDate = null;

            int logID = UserFunctions.InsertLog(ipAddress, "Home/Index", sourceId, sourceName, requestBody);

            UserFunctions.GetTellerTransactions(logID, sourceId, branch, startDate, endDate, out List<TellerTransaction> tellerTransactions, out decimal totalTransactionValue, out int totalTransactionVolume);

            ViewBag.TotalTransactionValue = totalTransactionValue;

            ViewBag.TotalTransactionVolume = totalTransactionVolume;

            UserFunctions.UpdateLogs(logID, StaticVariables.FAILSTATUS, JsonConvert.SerializeObject(tellerTransactions));

            return View();
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult GetTransactionsServerSide()
        {
            //Server Side Parameter
            CultureInfo culture = new CultureInfo("en-US");
            int start = Convert.ToInt32(Request["start"], culture);
            int length = Convert.ToInt32(Request["length"], culture);
            string searchValue = Request["search[value]"];
            string sortColumnName = Request["columns[" + Request["order[0][column]"] + "][name]"];
            string sortDirection = Request["order[0][dir]"];

            int totalCount = 0;

            int totalrowsafterfiltering = 0;

            List<TellerTransaction> tellerTransactionsDev = null;

            var ipAddress = UserFunctions.GetIPAddress();
            string sourceId = Session["SourceID"].ToString();
            string sourceName = Session["SourceName"].ToString();
            string branch = Session["Branch"].ToString();

            string requestBody = string.Empty;

            DateTime? startDate = null;

            DateTime? endDate = null;

            int logID = UserFunctions.InsertLog(ipAddress, "GetTransactionsServerSide", sourceId, sourceName, sourceId);

            if (UserFunctions.GetTellerTransactions(logID, sourceId, branch, startDate, endDate, out List<TellerTransaction> tellerTransactions, out decimal totalTransactionValue, out int totalTransactionVolume))
            {
                UserFunctions.TellerTransaction(0, tellerTransactions, searchValue, sortColumnName, sortDirection, start, length, out totalCount, out totalrowsafterfiltering, out tellerTransactionsDev);

                tellerTransactionsDev.ForEach(x => x.PrintAction = "<a class='fa fa-print' href='" + this.Url.Action("GenerateReceipt", "Home", new { x.Id }) + "'>Print Receipt</a>");

                var response = new { data = tellerTransactionsDev, draw = Request["draw"], recordsTotal = totalCount, recordsFiltered = totalrowsafterfiltering };

                UserFunctions.UpdateLogs(logID, StaticVariables.SUCCESSSTATUS, JsonConvert.SerializeObject(response));

                ViewBag.TotalTransactionValue = totalTransactionValue;

                ViewBag.TotalTransactionVolume = totalTransactionVolume;

                return Json(response, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var response = new { data = tellerTransactionsDev, draw = Request["draw"], recordsTotal = totalCount, recordsFiltered = totalrowsafterfiltering };

                UserFunctions.UpdateLogs(logID, StaticVariables.FAILSTATUS, JsonConvert.SerializeObject(response));

                ViewBag.TotalTransactionValue = totalTransactionValue;

                ViewBag.TotalTransactionVolume = totalTransactionVolume;

                return Json(response, JsonRequestBehavior.AllowGet);

            }


        }


        public ActionResult GenerateReceipt(int id)
        {
            string sourceId = "12345";

            string sourceName = "John Amoah";

            var ipAddress = UserFunctions.GetIPAddress();

            int logID = UserFunctions.InsertLog(ipAddress, "GenerateReceipt", sourceId, sourceName, JsonConvert.SerializeObject(id));


            if (UserFunctions.GetTellerTransaction(logID, id, out DateTime? transactionDate, out string idenntifier, out string thirdPartyReference, out string transactionType, out decimal amount, out string corebankingReference, out string branch))
            {
                ViewBag.Idenntifier = idenntifier;
                ViewBag.ThirdPartyReference = thirdPartyReference;
                ViewBag.TransactionType = transactionType;
                ViewBag.Amount = amount;
                ViewBag.CorebankingReference = corebankingReference;
                ViewBag.Branch = branch;
                ViewBag.TransactionDate = transactionDate;

                UserFunctions.UpdateLogs(logID, StaticVariables.SUCCESSSTATUS, StaticVariables.SUCCESSSTATUSMASSAGE);
            }
            else
            {
                ViewBag.ThirdPartyReference = thirdPartyReference;
                ViewBag.TransactionType = transactionType;
                ViewBag.Amount = amount;
                ViewBag.CorebankingReference = corebankingReference;
                ViewBag.Branch = branch;

                UserFunctions.UpdateLogs(logID, StaticVariables.FAILSTATUS, StaticVariables.FAILSTATUSMASSAGE);

            }


            return View();
        }
    }
}