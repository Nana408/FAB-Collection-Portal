using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAB_Merchant_Portal.Models
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "UserName is required")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class TellerTransaction
    {
        public int Id { get; set; }
        public decimal? Amount { get; set; }
        public string TransactionType { get; set; }
        public DateTime? EntryDate { get; set; }
        public string ThirdPartyReferece { get; set; }
        public string CorebankingReference { get; set; }
        public string TransactionStatus { get; set; }

        public string PrintAction { get; set; }

    }


    public class TokenResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string userName { get; set; }
        public string issued { get; set; }
        public string expires { get; set; }
    }
  

          public class VerifyGhanaGovInvoiceRequest
    {
        public string InvoiceNumber { get; set; }
        public string PointingAccountReference { get; set; }
       
    }

    public class VerifyGhanaGovInvoiceResponse
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public SearchInvoiceResponseObject SearchInvoiceResponseObject { get; set; }
    }

    public class GenericCenntralApiResponse
    {
        public int Status { get; set; }
        public string Message { get; set; }
       
    }
    public class SearchInvoiceResponseObject
    {
        public string InvoiceNumbber { get; set; }
        public string InvoiceStatus { get; set; }
        public string TotalAmount { get; set; }
        public string TotalAmountCurrency { get; set; }
        public string InvoiceDescription { get; set; }
        public string ExpiryDate { get; set; }
        public string CreatedDate { get; set; }
        public decimal PointingAccountAmount { get; set; }
        public string PointingAccountRemarks { get; set; }

    }

    public class PayGhanaGovInvoiceRequest
    {


        [Required]
        public string InvoiceNumber { get; set; }

        [Required]
        public string Currency { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public string BankBanchSortCode { get; set; }
        public string AccountNumber { get; set; }
        public string ChequeNumber { get; set; }
        public string ValueDate { get; set; }
    }

    public class GhanaGovPayInvoiceRequest
    {
        [Required]
        public string RequestorId { get; set; }

        [Required]
        public string InvoiceNumber { get; set; }

        [Required]
        public string Currency { get; set; }

        [Required]
        public string PaymentReference { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string AccountNumber { get; set; }

        public ChequeDetails ChequeDetails { get; set; }

    }


    public class ChequeDetails
    {
        public string BankBanchSortCode { get; set; }
        public string AccountNumber { get; set; }
        public string ChequeNumber { get; set; }
        public string ValueDate { get; set; }
    }

    public class PayGhanaGovInvoiceResponse
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public PayInvoiceResponseObject PayInvoiceResponseObject { get; set; }
    }


    public class PayInvoiceResponseObject
    {

        public string InvoiceNumber { get; set; }
        public int PaymentStatusCode { get; set; }
        public string PaymentStatusText { get; set; }
        public string PaymentReference { get; set; }
        public string DateProcessed { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
    }
}