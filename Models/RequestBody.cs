using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
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
        public string ChannelId { get; set; } = ConfigurationManager.AppSettings["CoreBankingChannelId"];
        public string AppId { get; set; } = ConfigurationManager.AppSettings["CoreBankingChannelId"];
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
     

    }

    public class PayGhanaGovInvoiceRequest
    {


        [Required]
        public string InvoiceNumber { get; set; }

        [Required]
        public string Currency { get; set; }

        [Required]
        public decimal Amount { get; set; }
        public string PointingAccountReference { get; set; }
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
        public string PointingAccountReference { get; set; }

        public ChequeDetails ChequeDetails { get; set; }

    }


    public class ChequeDetails
    {
        public string BankBanchSortCode { get; set; }
        public string AccountNumber { get; set; }
        public string ChequeNumber { get; set; }
        public string ValueDate { get; set; }
    }

    public class LoginDetails
    {
        public string Branch { get; set; } = "Head Office";
        public string SourceName { get; set; } = "John Amoah";
        public string SourceID { get; set; } = "12345";
        public string Till { get; set; } = "12345123332";
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



    public class LoginObject
    {
        public string STATUS { get; set; }
        public MESSAGE MESSAGE { get; set; }
    }

    public class MESSAGE
    {
        public Userdata UserData { get; set; }
        public Usermenu[] UserMenu { get; set; }
    }

    public class Userdata
    {
        public string UserId { get; set; }
        public object AppId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public object MiddleName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public object RoleList { get; set; }
        public string UserBranch { get; set; }
        public object BranchList { get; set; }
        public string UserEmail { get; set; }
        public object Password { get; set; }
        public object PwdChangeFlag { get; set; }
        public object PwdChangeDate { get; set; }
        public object PwdResetDate { get; set; }
        public object PwdExpiryDate { get; set; }
        public object LastPassword { get; set; }
        public object ADLogin { get; set; }
        public object AuthTypes { get; set; }
        public string UserStatus { get; set; }
        public DateTime LastLogin { get; set; }
        public int LoginAttempt { get; set; }
        public DateTime MakerDate { get; set; }
        public object MakerDateStamp { get; set; }
        public string MakerId { get; set; }
        public string AuthStat { get; set; }
        public string CheckerId { get; set; }
        public DateTime CheckerDateStamp { get; set; }
        public object DateModified { get; set; }
        public object UserModified { get; set; }
        public string RecordStat { get; set; }
        public int ModNo { get; set; }
        public object AutoAuth { get; set; }
        public object PointingAccount { get; set; } = "0051831301012";
    }

    public class Usermenu
    {
        public string MenuId { get; set; }
        public string AppId { get; set; }
        public object AppList { get; set; }
        public string MenuName { get; set; }
        public string Controller { get; set; }
        public string MenuAction { get; set; }
        public string RecordStat { get; set; }
        public DateTime MakerDate { get; set; }
        public DateTime MakerDateStamp { get; set; }
        public object MakerId { get; set; }
        public string AuthStat { get; set; }
        public object CheckerId { get; set; }
        public object CheckerDateStamp { get; set; }
        public object DateModified { get; set; }
        public object UserModified { get; set; }
        public int ModNo { get; set; }
        public object[] SubMenus { get; set; }
        public string RecordId { get; set; }
    }



    public class PointingAccountObject
    {
        public string STATUS { get; set; }
        public POINTINACCOUNTGMESSAGE MESSAGE { get; set; }
    }

    public class POINTINACCOUNTGMESSAGE
    {
        public DateTime PostDate { get; set; }
        public DateTime ValueDate { get; set; }
        public string Description { get; set; }
        public string Currency { get; set; }
        public decimal TrxnAmount { get; set; }
        public string CrDr { get; set; }
    }

}