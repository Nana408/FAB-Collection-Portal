using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FAB_Merchant_Portal.Models
{
    public static class StaticVariables
    {
        public static int  SUCCESSSTATUS = 1;

        public static string SUCCESSSTATUSMASSAGE = "Request Successful";

        public static int  FAILSTATUS = 0;

        public static string FAILSTATUSMASSAGE = "Request Failed";

        public static string LOGINATTEMPTFAILED = "Login attempt failed, check credentials again";


        public static string SERVERERRORSTATUS = "2";

        public static string SERVERERRORMASSAGE = "Unable to connect to remote host";

        public static string EXCEPTIONERROR = "ERROR";

        public static string COREBANKINGSUCCESSSTATUS = "SUCCESS";

        public static string COREBANKING = "Core Banking";

        public static string SERVERERRORMESSAGE = "Connection was lost";

        public static string INVALIDPOINTINGACCOUNT = "Invalid credit reference provided";

              public static string DUPLICATEPOINTINGACCOUNT = "Pointing Account Reference Already Exists";
    }
}