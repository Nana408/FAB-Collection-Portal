using FAB_Merchant_Portal.Helpers;
using FAB_Merchant_Portal.Models;
using Newtonsoft.Json;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace FAB_Merchant_Portal.Controllers
{

    public class GhanaGovController : Controller
    {
        // GET: GhanaGov
        [CheckSessionOut]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [CheckSessionOut]
        public ActionResult VerifyInvoice(VerifyGhanaGovInvoiceRequest request)
        {
            string sourceId = Session["SourceID"].ToString();
            string sourceName = Session["SourceName"].ToString();
            string branch = Session["Branch"].ToString();
            string accountNumberToDebit = Session["Till"].ToString();

            var ipAddress = UserFunctions.GetIPAddress();
            int logID = UserFunctions.InsertLog(ipAddress, "VerifyInvoice", sourceId, sourceName, request.InvoiceNumber);


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
            bool VerifyPointingAccountReference = UserFunctions.VerifyPointingAccountReference(logID, request.PointingAccountReference, accountNumberToDebit, out decimal PointingReferenceAmount, out string PointingReferenceRemarks, out message) && string.IsNullOrEmpty(request.PointingAccountReference);

            if (PointingReferenceAmount <= 0)
            {
                UserFunctions.UpdateLogs(logID, StaticVariables.FAILSTATUS, message);
                var error = new { Status = StaticVariables.FAILSTATUS, Message = message };
                return Json(error, JsonRequestBehavior.AllowGet);
            }

            if (UserFunctions.VerifyGhanaGov(logID, request.InvoiceNumber, out string PaidStatus, out string TotalAmount, out string Currency, out string Description, out string ExpiryDate, out message))
            {
                if (PaidStatus.ToUpper().Equals(StaticVariables.PAIDSTATUS))
                {
                    var data = new { Status = StaticVariables.FAILSTATUS, Message = StaticVariables.INVOICEALREADYPAID, request.InvoiceNumber, request.PointingAccountReference, PaidStatus, TotalAmount, Currency, Description, ExpiryDate, PointingReferenceAmount, PointingReferenceRemarks };
                    UserFunctions.UpdateLogs(logID, StaticVariables.FAILSTATUS, JsonConvert.SerializeObject(data));
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var data = new { Status = StaticVariables.SUCCESSSTATUS, Message = message, request.InvoiceNumber, request.PointingAccountReference, PaidStatus, TotalAmount, Currency, Description, ExpiryDate, PointingReferenceAmount, PointingReferenceRemarks };
                    UserFunctions.UpdateLogs(logID, StaticVariables.FAILSTATUS, JsonConvert.SerializeObject(data));
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                var data = new { Status = StaticVariables.FAILSTATUS, Message = message, request.InvoiceNumber, request.PointingAccountReference, PaidStatus, TotalAmount, Currency, Description, ExpiryDate, PointingReferenceAmount, PointingReferenceRemarks };
                UserFunctions.UpdateLogs(logID, StaticVariables.FAILSTATUS, JsonConvert.SerializeObject(data));
                return Json(data, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        [CheckSessionOut]
        public ActionResult PayInvoice(PayGhanaGovInvoiceRequest request)
        {
            string sourceId = Session["SourceID"].ToString();
            string sourceName = Session["SourceName"].ToString();
            string branch = Session["Branch"].ToString();
            string accountNumberToDebit = Session["Till"].ToString();

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


            if (UserFunctions.PayGhanaGov(logID, sourceId, request.InvoiceNumber, request.Amount, request.Currency, request.AccountNumber, request.BankBanchSortCode, request.ChequeNumber, request.ValueDate, accountNumberToDebit, request.Remarks, branch, out int transactionId, out message))
            {
                var data = new { Status = StaticVariables.SUCCESSSTATUS, Message = message, TransactionId = transactionId, RedirectURL = Url.Action("GenerateReceipt", "Home", new { id = transactionId }) };

                UserFunctions.UpdateLogs(logID, StaticVariables.FAILSTATUS, JsonConvert.SerializeObject(data));

                Task.Factory.StartNew(() => UserFunctions.LogPointingAccountReference(logID, request.PointingAccountReference, accountNumberToDebit, sourceName, true));

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var data = new { Status = StaticVariables.FAILSTATUS, Message = message, TransactionId = transactionId };

                UserFunctions.UpdateLogs(logID, StaticVariables.FAILSTATUS, JsonConvert.SerializeObject(data));

                Task.Factory.StartNew(() => UserFunctions.LogPointingAccountReference(logID, request.PointingAccountReference, accountNumberToDebit, sourceName, false));


                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }
    }
}