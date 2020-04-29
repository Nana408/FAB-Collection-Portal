using FAB_Merchant_Portal.Helpers;
using FAB_Merchant_Portal.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace FAB_Merchant_Portal.Controllers
{
  
    public class GhanaGovController : Controller
    {
        // GET: GhanaGov
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult VerifyInvoice(string invoiceNumber)
        {
            string sourceId = "12345";
            string sourceName = "John Amoah";


            var ipAddress = UserFunctions.GetIPAddress();
            int logID = UserFunctions.InsertLog(ipAddress, "VerifyInvoice", sourceId, sourceName, invoiceNumber);

            Thread.Sleep(3000);

            if (UserFunctions.VerifyGhanaGov(logID, invoiceNumber, out string PaidStatus, out string TotalAmount, out string Currency, out string Description, out string ExpiryDate, out string message))
            {
                var data = new { Status = StaticVariables.SUCCESSSTATUS, Message = message, InvoiceNumber = invoiceNumber, PaidStatus, TotalAmount, Currency, Description, ExpiryDate };
                UserFunctions.UpdateLogs(logID, StaticVariables.FAILSTATUS, JsonConvert.SerializeObject(data));
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var data = new { Status = StaticVariables.FAILSTATUS, Message = message, InvoiceNumber = invoiceNumber, PaidStatus, TotalAmount, Currency, Description, ExpiryDate };
                UserFunctions.UpdateLogs(logID, StaticVariables.FAILSTATUS, JsonConvert.SerializeObject(data));
                return Json(data, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult PayInvoice(PayGhanaGovInvoiceRequest request)
        {
            string sourceId = "12345";

            string sourceName = "John Amoah";



            var ipAddress = UserFunctions.GetIPAddress();

            int logID = UserFunctions.InsertLog(ipAddress, "PayInvoice", sourceId, sourceName, JsonConvert.SerializeObject(request));

            if (request == null)
            {
                UserFunctions.UpdateLogs(logID, StaticVariables.FAILSTATUS, StaticVariables.FAILSTATUSMASSAGE);

                var error = new { Status = StaticVariables.FAILSTATUS, Message = StaticVariables.FAILSTATUSMASSAGE };

                return Json(error, JsonRequestBehavior.AllowGet);
            }


            string message;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.ElementAt(0);

                var messList = errors.Errors.ElementAt(0);

                message = messList.ErrorMessage;

                UserFunctions.UpdateLogs(logID, StaticVariables.FAILSTATUS, message);

                var error = new { Status = StaticVariables.FAILSTATUS, Message = message };

                return Json(error, JsonRequestBehavior.AllowGet);
            }


            if (UserFunctions.PayGhanaGov(logID, sourceId, request.InvoiceNumber, request.Amount, request.Currency, request.AccountNumber, request.BankBanchSortCode, request.ChequeNumber, request.ValueDate, out int transactionId, out message))
            {
                var data = new { Status = StaticVariables.SUCCESSSTATUS, Message = message, TransactionId = transactionId , RedirectURL = Url.Action("GenerateReceipt", "Home",new { id=transactionId}) };
                UserFunctions.UpdateLogs(logID, StaticVariables.FAILSTATUS, JsonConvert.SerializeObject(data));
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var data = new { Status = StaticVariables.FAILSTATUS, Message = message, TransactionId = transactionId };
                UserFunctions.UpdateLogs(logID, StaticVariables.FAILSTATUS, JsonConvert.SerializeObject(data));
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }
    }
}