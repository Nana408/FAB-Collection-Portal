using FAB_Merchant_Portal.Helpers;
using FAB_Merchant_Portal.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Web.Mvc;

namespace FAB_Merchant_Portal.Controllers
{

    public class HomeController : Controller
    {
        [CheckSessionOut]
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

            ExportRequest exportRequest = new ExportRequest
            {
                StartDate = DateTime.Now.Date.ToString("dd/MM/yyy"),
                EndDate = DateTime.Now.Date.AddDays(1).ToString("dd/MM/yyy")
            };

            ViewBag.fromDate = exportRequest.StartDate;

            ViewBag.toDate = exportRequest.EndDate;

            return View(exportRequest);
  
        }

        [HttpPost]
        [CheckSessionOut]
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

                tellerTransactionsDev.ForEach(x => x.PrintAction = "<a class='fa fa-print' href='" + this.Url.Action("GenerateReceipt", "Home", new { x.Id }) + "' target = '_blank'>Print Receipt</a>");

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

        [CheckSessionOut]
        public ActionResult GenerateReceipt(int id)
        {
            string sourceId = "12345";

            string sourceName = "John Amoah";

            var ipAddress = UserFunctions.GetIPAddress();

            int logID = UserFunctions.InsertLog(ipAddress, "GenerateReceipt", sourceId, sourceName, JsonConvert.SerializeObject(id));


            if (UserFunctions.GetTellerTransaction(logID, id, out DateTime? transactionDate, out string idenntifier, out string thirdPartyReference, out string transactionType, out decimal amount, out string corebankingReference, out string branch, out string teller))
            {
                ViewBag.Idenntifier = idenntifier;
                ViewBag.ThirdPartyReference = thirdPartyReference;
                ViewBag.TransactionType = transactionType;
                ViewBag.Amount = amount;
                ViewBag.CorebankingReference = corebankingReference;
                ViewBag.Branch = branch;
                ViewBag.TransactionDate = transactionDate;
                ViewBag.TellerId = teller;

                UserFunctions.UpdateLogs(logID, StaticVariables.SUCCESSSTATUS, StaticVariables.SUCCESSSTATUSMASSAGE);
            }
            else
            {
                ViewBag.Identifier = idenntifier;
                ViewBag.ThirdPartyReference = thirdPartyReference;
                ViewBag.TransactionType = transactionType;
                ViewBag.Amount = amount;
                ViewBag.CorebankingReference = corebankingReference;
                ViewBag.Branch = branch;
                ViewBag.TransactionDate = transactionDate;
                ViewBag.TellerId = teller;

                UserFunctions.UpdateLogs(logID, StaticVariables.FAILSTATUS, StaticVariables.FAILSTATUSMASSAGE);

            }


            return View();
        }

        [HttpGet]
        [CheckSessionOut]
        public ActionResult ExportExcel(string StartDate, string EndDate)
        {
            // This is the query result set the user wishes to export to file.
            string title = "AUD FROM " + StartDate + " to " + EndDate;

            DateTime sDate = DateTime.ParseExact(StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            DateTime eDate = DateTime.ParseExact(EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            IEnumerable<ExportDetailsVM> exportQuery = UserFunctions.GetRecordsWithinPriods(sDate, eDate);
            string imagePath = @"E:\wwwroot\AUDWEB\Content\Theme\LandingTheme\assets\images\FAB.png";

            byte[] response;
            using (var excelFile = new ExcelPackage())
            {
                excelFile.Workbook.Properties.Title = title;
                var worksheet = excelFile.Workbook.Worksheets.Add("Sheet1");
                UserFunctions.AddImage(worksheet, 1, 1, imagePath);

                worksheet.Cells["A8:M12"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["A8:M12"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["A8:M12"].Merge = true;
                worksheet.Cells["A8:M12"].Value = "Acceptable User Declaration";
                worksheet.Cells["A8:M12"].Style.Font.Size = 25;
                worksheet.Cells["A8:M12"].Style.Font.Name = "Calibri";
                worksheet.Cells["A8:M12"].Style.Font.Bold = true;
                worksheet.Cells["A8:M12"].Style.Font.Color.SetColor(Color.Purple);
                worksheet.Row(14).Style.Font.Bold = true;
                worksheet.Cells["A14"].LoadFromCollection(Collection: exportQuery, PrintHeaders: true);
                response = excelFile.GetAsByteArray();
            }

            // Save dialog appears through browser for user to save file as desired.
            return File(response, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", title + ".xlsx");
        }

        [HttpGet]
        [CheckSessionOut]
        public FileStreamResult ExportPdf(string StartDate, string EndDate)
        {

            DateTime sDate = DateTime.ParseExact(StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            DateTime eDate = DateTime.ParseExact(EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            var getListOfRecords = (UserFunctions.GetRecordsWithinPriod(sDate, eDate));

            DataTable dataTable = null;

            if (getListOfRecords != null)
            {
                dataTable = UserFunctions.ToDataTable(getListOfRecords);
            }

            MemoryStream workStream = new MemoryStream();

            Document document = new Document(new iTextSharp.text.Rectangle(288f, 144f), 10, 10, 10, 10);

            document.SetPageSize(PageSize.A4.Rotate());

            PdfWriter.GetInstance(document, workStream).CloseStream = false;

            document.Open();

            string imageURL = @"E:\wwwroot\AUDWEB\Content\Theme\LandingTheme\assets\images\FAB.png";

            iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(imageURL);

            //Resize image depend upon your need

            jpg.ScaleToFit(140f, 120f);

            //Give space before image

            jpg.SpacingBefore = 10f;

            //Give some space after the image

            jpg.SpacingAfter = 1f;

            jpg.Alignment = Element.ALIGN_LEFT;

            document.Add(jpg);

            iTextSharp.text.Font titleFont = FontFactory.GetFont("Arial", 32, iTextSharp.text.Font.BOLD | iTextSharp.text.Font.UNDERLINE, new BaseColor(Color.Purple));

            Paragraph title;

            title = new Paragraph("Acceptable User Declaration", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,

                SpacingAfter = 20,

            };
            document.Add(title);

            PdfPTable table = new PdfPTable(dataTable.Columns.Count)
            {
                WidthPercentage = 100
            };

            //Set columns names in the pdf file
            for (int k = 0; k < dataTable.Columns.Count; k++)
            {
                PdfPCell cell = new PdfPCell(new Phrase(UserFunctions.GetColumnActualName(dataTable.Columns[k].ColumnName)))
                {
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    VerticalAlignment = PdfPCell.ALIGN_CENTER,
                    BackgroundColor = new BaseColor(Color.Purple)
                };

                table.AddCell(cell);
            }

            //Add values of DataTable in pdf file
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(dataTable.Rows[i][j].ToString()))
                    {
                        //Align the cell in the center
                        HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                        VerticalAlignment = PdfPCell.ALIGN_CENTER
                    };

                    table.AddCell(cell);
                }
            }

            document.Add(table);
            document.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return new FileStreamResult(workStream, "application/pdf");
        }


        [CheckSessionOut]
        public ActionResult BranchReport()
        {
            var ipAddress = UserFunctions.GetIPAddress();

            string sourceId = Session["SourceID"].ToString();
            string sourceName = Session["SourceName"].ToString();
            string branch = Session["Branch"].ToString();

            string requestBody = string.Empty;

            DateTime? startDate = null;

            DateTime? endDate = null;

            int logID = UserFunctions.InsertLog(ipAddress, "Home/BranchReport", sourceId, sourceName, requestBody);

            UserFunctions.GetBranchTransactions(logID, sourceId, branch, startDate, endDate, out List<TellerTransaction> tellerTransactions, out decimal totalTransactionValue, out int totalTransactionVolume);

            ViewBag.TotalTransactionValue = totalTransactionValue;

            ViewBag.TotalTransactionVolume = totalTransactionVolume;

            UserFunctions.UpdateLogs(logID, StaticVariables.FAILSTATUS, JsonConvert.SerializeObject(tellerTransactions));

            ExportRequest exportRequest = new ExportRequest
            {
                StartDate = DateTime.Now.Date.ToString("dd/MM/yyy"),
                EndDate = DateTime.Now.Date.AddDays(1).ToString("dd/MM/yyy")
            };

            ViewBag.fromDate = exportRequest.StartDate;

            ViewBag.toDate = exportRequest.EndDate;

            return View(exportRequest);

        }

        [HttpPost]
        [CheckSessionOut]
        //[ValidateAntiForgeryToken]
        public ActionResult GetBranchReportTransactionsServerSide()
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

            int logID = UserFunctions.InsertLog(ipAddress, "GetBranchReportTransactionsServerSide", sourceId, sourceName, sourceId);

            if (UserFunctions.GetBranchTransactions(logID, sourceId, branch, startDate, endDate, out List<TellerTransaction> tellerTransactions, out decimal totalTransactionValue, out int totalTransactionVolume))
            {
                UserFunctions.TellerTransaction(0, tellerTransactions, searchValue, sortColumnName, sortDirection, start, length, out totalCount, out totalrowsafterfiltering, out tellerTransactionsDev);

                tellerTransactionsDev.ForEach(x => x.PrintAction = "<a class='fa fa-print' href='" + this.Url.Action("GenerateReceipt", "Home", new { x.Id }) + "' target = '_blank'>Print Receipt</a>");

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


        [CheckSessionOut]
        public ActionResult BankReport()
        {
            var ipAddress = UserFunctions.GetIPAddress();

            string sourceId = Session["SourceID"].ToString();
            string sourceName = Session["SourceName"].ToString();     

            string requestBody = string.Empty;

            DateTime? startDate = null;

            DateTime? endDate = null;

            int logID = UserFunctions.InsertLog(ipAddress, "Home/BankReport", sourceId, sourceName, requestBody);

            UserFunctions.GetBankTransactions(logID, sourceId, startDate, endDate, out List<TellerTransaction> tellerTransactions, out decimal totalTransactionValue, out int totalTransactionVolume);

            ViewBag.TotalTransactionValue = totalTransactionValue;

            ViewBag.TotalTransactionVolume = totalTransactionVolume;

            UserFunctions.UpdateLogs(logID, StaticVariables.FAILSTATUS, JsonConvert.SerializeObject(tellerTransactions));

            ExportRequest exportRequest = new ExportRequest
            {
                StartDate = DateTime.Now.Date.ToString("dd/MM/yyy"),
                EndDate = DateTime.Now.Date.AddDays(1).ToString("dd/MM/yyy")
            };

            ViewBag.fromDate = exportRequest.StartDate;

            ViewBag.toDate = exportRequest.EndDate;

            return View(exportRequest);

        }

        [HttpPost]
        [CheckSessionOut]
        //[ValidateAntiForgeryToken]
        public ActionResult GetBankReportTransactionsServerSide()
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

            int logID = UserFunctions.InsertLog(ipAddress, "GetBankReportTransactionsServerSide", sourceId, sourceName, sourceId);

            if (UserFunctions.GetBankTransactions(logID, sourceId, startDate, endDate, out List<TellerTransaction> tellerTransactions, out decimal totalTransactionValue, out int totalTransactionVolume))
            {
                UserFunctions.TellerTransaction(0, tellerTransactions, searchValue, sortColumnName, sortDirection, start, length, out totalCount, out totalrowsafterfiltering, out tellerTransactionsDev);

                tellerTransactionsDev.ForEach(x => x.PrintAction = "<a class='fa fa-print' href='" + this.Url.Action("GenerateReceipt", "Home", new { x.Id }) + "' target = '_blank'>Print Receipt</a>");

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



        [CheckSessionOut]
        public ActionResult AuditReport()
        {
            var ipAddress = UserFunctions.GetIPAddress();

            string sourceId = Session["SourceID"].ToString();
            string sourceName = Session["SourceName"].ToString();

            string requestBody = string.Empty;

            DateTime? startDate = null;

            DateTime? endDate = null;

            int logID = UserFunctions.InsertLog(ipAddress, "Home/AuditReport", sourceId, sourceName, requestBody);

            UserFunctions.GetAuditLogs(logID, sourceId, startDate, endDate, out List<AuditLog> tellerTransactions,  out int totalTransactionVolume);

            ViewBag.TotalTransactionVolume = totalTransactionVolume;

          
            ExportRequest exportRequest = new ExportRequest
            {
                StartDate = DateTime.Now.Date.ToString("dd/MM/yyy"),
                EndDate = DateTime.Now.Date.AddDays(1).ToString("dd/MM/yyy")
            };

            ViewBag.fromDate = exportRequest.StartDate;

            ViewBag.toDate = exportRequest.EndDate;

            UserFunctions.UpdateLogs(logID, StaticVariables.SUCCESSSTATUS, JsonConvert.SerializeObject(tellerTransactions));

            return View(exportRequest);

        }

        [HttpPost]
        [CheckSessionOut]
        //[ValidateAntiForgeryToken]
        public ActionResult GetAuditReportsServerSide()
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

            List<AuditLog> tellerTransactionsDev = null;

            var ipAddress = UserFunctions.GetIPAddress();
            string sourceId = Session["SourceID"].ToString();
            string sourceName = Session["SourceName"].ToString();
            string branch = Session["Branch"].ToString();

            string requestBody = string.Empty;

            DateTime? startDate = null;

            DateTime? endDate = null;

            int logID = UserFunctions.InsertLog(ipAddress, "GetAuditReportsServerSide", sourceId, sourceName, sourceId);

            if (UserFunctions.GetAuditLogs( logID,  sourceId, startDate, endDate, out List<AuditLog> auditLogs, out int totalTransactionVolume))
            {
                UserFunctions.AuditLog(logID, auditLogs, searchValue, sortColumnName, sortDirection, start, length, out totalCount, out totalrowsafterfiltering, out tellerTransactionsDev);

                var response = new { data = tellerTransactionsDev, draw = Request["draw"], recordsTotal = totalCount, recordsFiltered = totalrowsafterfiltering };

                tellerTransactionsDev.ForEach(x => x.ActivityResponse = UserFunctions.GetLogStatus(x.ActivityStatus));

               
                UserFunctions.UpdateLogs(logID, StaticVariables.SUCCESSSTATUS, JsonConvert.SerializeObject(response));
   

                ViewBag.TotalTransactionVolume = totalTransactionVolume;

                return Json(response, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var response = new { data = tellerTransactionsDev, draw = Request["draw"], recordsTotal = totalCount, recordsFiltered = totalrowsafterfiltering };

                UserFunctions.UpdateLogs(logID, StaticVariables.FAILSTATUS, JsonConvert.SerializeObject(response));

             

                ViewBag.TotalTransactionVolume = totalTransactionVolume;

                return Json(response, JsonRequestBehavior.AllowGet);

            }


        }

    }
}