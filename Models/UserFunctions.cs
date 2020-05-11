using FAB_Merchant_Portal.DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Linq.Dynamic;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Runtime.CompilerServices;

namespace FAB_Merchant_Portal.Models
{
    public static class UserFunctions
    {

        public static bool GetTellerTransactions(int logId, string sourceId, string branch, DateTime? startDate, DateTime? endDate, out List<TellerTransaction> tellerTransactions, out decimal totalTransactionValue, out int totalTransactionVolume)
        {
            tellerTransactions = new List<TellerTransaction>();

            totalTransactionValue = 0;

            totalTransactionVolume = 0;

            bool worked = false;
            try
            {
                using (FABMerchantPortalDBEntities db = new FABMerchantPortalDBEntities())
                {
                    tellerTransactions = db.TransactionLogs.Where(x => x.SourceID == sourceId && x.Branch.ToUpper().Trim().Equals(branch.ToUpper().Trim())).Select(x => new TellerTransaction { Id = x.Id, TransactionStatus = x.TransactionStatus, Amount = x.Amount, CorebankingReference = x.CoreBankingReference, EntryDate = x.EntryDate, ThirdPartyReferece = x.ThirdPartyReference, TransactionType = x.TransactionType }).ToList();

                    if (tellerTransactions != null)
                    {
                        if (startDate != null && endDate != null)
                        {
                            tellerTransactions = tellerTransactions.Where(xx => xx.EntryDate >= startDate && xx.EntryDate <= endDate).OrderByDescending(x => x.EntryDate).ToList();
                        }
                        else
                        {
                            tellerTransactions = tellerTransactions.Where(xx => xx.EntryDate >= DateTime.Today && xx.EntryDate <= DateTime.Today.AddHours(24)).OrderByDescending(x => x.EntryDate).ToList();
                        }

                        totalTransactionValue = (decimal)tellerTransactions.Sum(x => x.Amount);

                        totalTransactionVolume = tellerTransactions.Count();
                        worked = true;
                    }

                }
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() => WriteLog(logId.ToString(), sourceId, ex.Message, StaticVariables.EXCEPTIONERROR, string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name)));


            }
            return worked;
        }
        public static bool InsertTellerTransactions(string branch,string invoiceReference,string responseJson, string transactionStatus, string thirdPartyReference, string transactionType, int logId, decimal amount, string sourceId, string corebankingReference, out int transactionId)
        {
            bool worked = false;
            TransactionLog transactionLog = new TransactionLog();
            transactionId = 0;
            try
            {
                using (FABMerchantPortalDBEntities db = new FABMerchantPortalDBEntities())
                {
                    transactionLog = new TransactionLog()
                    {
                        
                        Amount = amount,
                        CoreBankingReference = corebankingReference,
                        CreatedBy = sourceId,
                        CreatedDate = DateTime.UtcNow,
                        EntryDate = DateTime.UtcNow,
                        SourceID = sourceId,
                        LogId = logId,
                        TransactionType = transactionType,
                        ThirdPartyReference = thirdPartyReference,
                        TransactionStatus = transactionStatus,
                        ResponseJson = responseJson,
                        Identifier = invoiceReference,
                        Branch= branch
                        
                    };

                    db.TransactionLogs.Add(transactionLog);
                    db.SaveChanges();
                    transactionId = transactionLog.Id;

                }
            }
            catch (Exception ex)
            {

                Task.Factory.StartNew(() => WriteLog(logId.ToString(), JsonConvert.SerializeObject(transactionLog), ex.Message, StaticVariables.EXCEPTIONERROR, string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name)));

            }
            return worked;
        }

        public static bool GetTellerTransaction(int logId, int id, out DateTime? transactionDate, out string idenntifier, out string thirdPartyReference, out string transactionType, out decimal amount, out string corebankingReference, out string branch)
        {
            bool worked = false;
            thirdPartyReference = string.Empty;
            amount = 0;
            transactionType = string.Empty;
            corebankingReference = string.Empty;
            branch = string.Empty;
            idenntifier = string.Empty;
            transactionDate = null;

            try
            {
                using (FABMerchantPortalDBEntities db = new FABMerchantPortalDBEntities())
                {
                    var transaction = db.TransactionLogs.Find(id);

                    if (transaction != null)
                    {
                        transactionDate = transaction.EntryDate;
                        idenntifier = transaction.Identifier;
                        thirdPartyReference = transaction.ThirdPartyReference;
                        amount = (decimal)transaction.Amount;
                        transactionType = transaction.TransactionType;
                        corebankingReference = transaction.CoreBankingReference;
                        branch = transaction.Branch;
                    }
                    worked = true;


                }
            }
            catch (Exception ex)
            {

                Task.Factory.StartNew(() => WriteLog(logId.ToString(), id.ToString(), ex.Message, StaticVariables.EXCEPTIONERROR, string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name)));

            }
            return worked;
        }

        public static void UpdateLogs(int id, int transStatus, string responseBody)
        {
            try
            {
                using (FABMerchantPortalDBEntities db = new FABMerchantPortalDBEntities())
                {
                    var userLog = db.UserLogs.Find(id);
                    userLog.TransStatus = transStatus.ToString();
                    userLog.ResponseBody = responseBody;
                    userLog.EndDate = DateTime.UtcNow;
                    db.Entry(userLog).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {

                Task.Factory.StartNew(() => WriteLog(id.ToString(), transStatus + " || " + responseBody, ex.Message, StaticVariables.EXCEPTIONERROR, string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name)));

            }

        }

        public static int InsertLog(string userAgent, string userFunction, string sourceID, string sourceName, string requestBody)
        {
            int id = 0;
            try
            {
                using (FABMerchantPortalDBEntities db = new FABMerchantPortalDBEntities())
                {

                    UserLog userLog = new UserLog
                    {
                        UserAgent = userAgent,
                        UserFunction = userFunction,
                        StartDate = DateTime.UtcNow,
                        RequestBody = requestBody,
                        SourceID = sourceID,
                        SourceName = sourceName

                    };
                    db.UserLogs.Add(userLog);
                    db.SaveChanges();
                    id = userLog.Id;

                }
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() => WriteLog(id.ToString(), requestBody, ex.Message, StaticVariables.EXCEPTIONERROR, string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name)));

            }
            return id;
        }

        public static void WriteLog(dynamic logId, string request, string response, string serviceName, string mfunctionName, [CallerMemberName] string callerName = "")
        {
            mfunctionName = callerName;
            string logFilePath = "C:\\Logs\\" + serviceName + "\\";
            logFilePath = logFilePath + "Log-" + DateTime.Today.ToString("MM-dd-yyyy") + "." + "txt";
            try
            {
                using (FileStream fileStream = new FileStream(logFilePath, FileMode.Append))
                {
                    FileInfo logFileInfo;


                    logFileInfo = new FileInfo(logFilePath);
                    DirectoryInfo logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
                    if (!logDirInfo.Exists) logDirInfo.Create();

                    StreamWriter log = new StreamWriter(fileStream);

                    if (!logFileInfo.Exists)
                    {
                        _ = logFileInfo.Create();
                    }
                    else
                    {


                        log.WriteLine(logId);
                        log.WriteLine(DateTime.UtcNow.ToString());
                        log.WriteLine(request);
                        log.WriteLine(response);
                        log.WriteLine(mfunctionName);
                        log.WriteLine("_________________________________________________________________________________________________________");
                        log.Close();

                    }
                    fileStream.Close();
                }
            }
            catch (Exception)
            {


            }

        }

        private const string HttpContext = "MS_HttpContext";
        private const string RemoteEndpointMessage = "System.ServiceModel.Channels.RemoteEndpointMessageProperty";
        public static string GetClientIpAddress(this HttpRequestMessage request)
        {
            System.Net.Http.Headers.HttpRequestHeaders headers = request.Headers;
            String agent = "";
            if (headers.Contains("User-Agent"))
            {
                agent = headers.GetValues("User-Agent").First();
            }

            if (request.Properties.ContainsKey(HttpContext))
            {
                dynamic ctx = request.Properties[HttpContext];
                if (ctx != null)
                {
                    return agent + "/" + ctx.Request.UserHostAddress;
                }
            }

            if (request.Properties.ContainsKey(RemoteEndpointMessage))
            {
                dynamic remoteEndpoint = request.Properties[RemoteEndpointMessage];
                if (remoteEndpoint != null)
                {
                    return agent + "/" + remoteEndpoint.Address;
                }
            }

            return agent;
        }

        public static string GetIPAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return context.Request.ServerVariables["REMOTE_ADDR"];
        }
        public static void TellerTransaction(int logId, List<TellerTransaction> answers, string searchValue, string sortColumnName, string sortDirection, int start, int length, out int totalCount, out int totalrowsafterfiltering, out List<TellerTransaction> tellerTransactions)
        {
            CultureInfo culture = new CultureInfo("en-US");
            totalCount = 0;
            totalrowsafterfiltering = 0;
            object transactionObbject = answers;
            tellerTransactions = null;
            try
            {
                totalCount = answers.Count;

                if (!string.IsNullOrEmpty(searchValue))//filter
                {
                    answers = answers.Where(x => x.CorebankingReference.ToLower(culture).Contains(searchValue.ToLower(culture)) || x.ThirdPartyReferece.ToLower(culture).Contains(searchValue.ToLower(culture)) || x.TransactionType.ToLower(culture).Contains(searchValue.ToLower(culture))).ToList();
                }
                totalrowsafterfiltering = answers.Count;

                //sorting
                answers = answers.OrderBy(sortColumnName + " " + sortDirection).ToList();

                //paging
                answers = answers.Skip(start).Take(length).ToList();

                tellerTransactions = answers;
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() => WriteLog(logId.ToString(), JsonConvert.SerializeObject(transactionObbject), ex.Message, StaticVariables.EXCEPTIONERROR, string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name)));

            }
        }

        public static bool VerifyGhanaGov(int logId, string invoice,out string PaidStatus, out string TotalAmount, out string Currency, out string Description, out string ExpiryDate,  out string message)
        {
            bool worked = false;
            PaidStatus = string.Empty;
            TotalAmount = string.Empty;
            Currency = string.Empty;
            Description = string.Empty;
            ExpiryDate = string.Empty;

         

            string apiURL = ConfigurationManager.AppSettings["apiURL"];
            string requestorId = ConfigurationManager.AppSettings["RequestorId"];
            string req = string.Empty;
            message = string.Empty;
            string apiResponse = string.Empty;
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                    HttpResponseMessage result = client.GetAsync(apiURL + "api/GhanaGov/Test").Result;

                    if (result.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        TokenResponse tokenResponse = RequestAccessToken(); /* Or reenter resource owner credentials if refresh token is not implemented */

                        if (tokenResponse != null)
                        {
                            _newAccessToken = tokenResponse.access_token;

                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _newAccessToken);

                            var json = new
                            {
                                RequestorId = requestorId,
                                InvoiceNumber = invoice,
                                InitiatorReference = logId
                              
                            };

                            req = JsonConvert.SerializeObject(json);

                            HttpContent content = new StringContent(JsonConvert.SerializeObject(json), Encoding.UTF8, "application/json");

                            var res = client.PostAsync(new Uri(apiURL + "api/GhanaGov/RetrieveInvoiceByInvoiceNumber"), content).Result;

                            string statusCod = res.StatusCode.ToString();

                            apiResponse = res.Content.ReadAsStringAsync().Result;

                            try
                            {

                                VerifyGhanaGovInvoiceResponse invoiceSearchResponse = JsonConvert.DeserializeObject<VerifyGhanaGovInvoiceResponse>(res.Content.ReadAsStringAsync().Result);

                                if (statusCod == "OK" && invoiceSearchResponse.status == 1)
                                {
                                    PaidStatus = invoiceSearchResponse.response.invoiceStatus;
                                    TotalAmount = invoiceSearchResponse.response.totalAmount;
                                    Currency = invoiceSearchResponse.response.totalAmountCurrency;
                                    Description = invoiceSearchResponse.response.invoiceDescription;
                                    ExpiryDate = invoiceSearchResponse.response.expiryDate;                                  
                                    message = invoiceSearchResponse.message;

                                    worked = true;
                                }
                                else
                                {

                                    message = invoiceSearchResponse.message;
                                }

                            }
                            catch (Exception)
                            {
                                GenericCenntralApiResponse invoiceSearchResponse = JsonConvert.DeserializeObject<GenericCenntralApiResponse>(res.Content.ReadAsStringAsync().Result);

                                message = invoiceSearchResponse.Message;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() => WriteLog(logId.ToString(), req, ex.Message, StaticVariables.EXCEPTIONERROR, string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name)));
            }

            Task.Factory.StartNew(() => WriteLog(logId, req, apiResponse, ConfigurationManager.AppSettings["GhanaGovService"], string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name)));

            return worked;
        }


        public static bool PayGhanaGov(int logId, string souceId, string invoice, decimal amount, string currency, string accountNuber, string bankBanchSortCode, string chequeNumber, string valueDate, string accountNumberToDebit,string remarks,string branch, out int transactionId, out string message)
        {
            bool worked = false;
            string apiURL = ConfigurationManager.AppSettings["apiURL"];
            string requestorId = ConfigurationManager.AppSettings["RequestorId"];
            string req = string.Empty;
            message = string.Empty;
            string apiResponse = string.Empty;
            transactionId = 0;
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                    HttpResponseMessage result = client.GetAsync(apiURL + "api/GhanaGov/Test").Result;

                    if (result.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        TokenResponse tokenResponse = RequestAccessToken(); /* Or reenter resource owner credentials if refresh token is not implemented */

                        if (tokenResponse != null)
                        {
                            _newAccessToken = tokenResponse.access_token;

                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _newAccessToken);


                            ChequeDetails chequeDetails = new ChequeDetails
                            {
                                AccountNumber = accountNuber,
                                BankBanchSortCode = bankBanchSortCode,
                                ChequeNumber = chequeNumber,
                                ValueDate = valueDate
                            };

                            GhanaGovPayInvoiceRequest json = new GhanaGovPayInvoiceRequest
                            {
                                RequestorId = requestorId,
                                InvoiceNumber = invoice,
                                Amount = amount,
                                Currency = currency,
                                PaymentReference = logId.ToString(),
                                AccountNumber = accountNumberToDebit,
                                Remarks = remarks,
                                ChequeDetails = chequeDetails
                            };

                            req = JsonConvert.SerializeObject(json);

                            HttpContent content = new StringContent(JsonConvert.SerializeObject(json), Encoding.UTF8, "application/json");

                            var res = client.PostAsync(new Uri(apiURL + "api/GhanaGov/PayInvoice"), content).Result;

                            string statusCod = res.StatusCode.ToString();

                            apiResponse = res.Content.ReadAsStringAsync().Result;

                            try
                            {

                                PayGhanaGovInvoiceResponse invoiceSearchResponse = JsonConvert.DeserializeObject<PayGhanaGovInvoiceResponse>(res.Content.ReadAsStringAsync().Result);

                                if (result.StatusCode != HttpStatusCode.Unauthorized && invoiceSearchResponse.Status == 1)
                                {

                                    message = invoiceSearchResponse.Message;

                                    string corebankingReference = string.Empty;

                                    InsertTellerTransactions(branch, invoice,apiResponse, StaticVariables.SUCCESSSTATUSMASSAGE, invoiceSearchResponse.PayInvoiceResponseObject.PaymentReference, ConfigurationManager.AppSettings["GhanaGovService"], logId, amount, souceId, corebankingReference, out transactionId);

                                    worked = true;
                                }
                                else
                                {

                                    message = invoiceSearchResponse.Message;
                                }

                            }
                            catch (Exception)
                            {
                                GenericCenntralApiResponse invoiceSearchResponse = JsonConvert.DeserializeObject<GenericCenntralApiResponse>(res.Content.ReadAsStringAsync().Result);

                                message = invoiceSearchResponse.Message;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() => WriteLog(logId.ToString(), req, ex.Message, StaticVariables.EXCEPTIONERROR, string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name)));
            }

            Task.Factory.StartNew(() => WriteLog(logId, req, apiResponse, ConfigurationManager.AppSettings["GhanaGovService"], string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name)));

            return worked;
        }

        static string _accessToken = string.Empty;

        static string _newAccessToken = string.Empty;
        public static TokenResponse RequestAccessToken()
        {
            string _user = ConfigurationManager.AppSettings["username"];
            string _pwd = ConfigurationManager.AppSettings["password"];
            string _clientId = ConfigurationManager.AppSettings["clientId"];
            string _clientSecret = ConfigurationManager.AppSettings["clientSecret"];
            string _tokenUrl = ConfigurationManager.AppSettings["tokenUrl"];

            using (var client = new HttpClient())
            {
                var postData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("username", _user),
                    new KeyValuePair<string, string>("password", _pwd),
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("client_id", _clientId),
                    new KeyValuePair<string, string>("client_secret", _clientSecret)
                };

                HttpContent content = new FormUrlEncodedContent(postData);

                content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                var responseResult = client.PostAsync(_tokenUrl, content).Result;

                TokenResponse tokenResponse = DeserialzetokenResponse(responseResult.Content.ReadAsStringAsync().Result.Replace(".", string.Empty));

                return tokenResponse;
            }

        }

        public static TokenResponse DeserialzetokenResponse(string inputstring)
        {
            TokenResponse tokenResponse = null;

            try
            {
                tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(inputstring);
            }
            catch (Exception)
            {

            }
            return tokenResponse;
        }

        public static string TestApi()
        {
            string apiURL = ConfigurationManager.AppSettings["apiURL"];
            string output = string.Empty;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                HttpResponseMessage result = client.GetAsync(apiURL + "/api/webapi").Result;

                if (result.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TokenResponse tokenResponse = RequestAccessToken(); /* Or reenter resource owner credentials if refresh token is not implemented */

                    if (tokenResponse != null)
                    {
                        _newAccessToken = tokenResponse.access_token;

                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _newAccessToken);

                        result = client.GetAsync(apiURL + "/api/webapi").Result;

                        if (result.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            output = result.Content.ReadAsStringAsync().Result;

                            // Process the error
                        }
                        else
                        {
                            output = result.Content.ReadAsStringAsync().Result;
                        }
                    }
                }

                return output;
            }
        }

        public static bool Login(int logId, string userName, string password, out LoginObject loginDetails, out string message)
        {
            bool worked = false;
            message = string.Empty;
            string reqeuest = string.Empty;
            string response = string.Empty;
            loginDetails = null;
            string url = ConfigurationManager.AppSettings["CoreBankingBaseURL"];
            string merchantbase64 = ConfigurationManager.AppSettings["CoreBankingAuthorizationKey"];

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", merchantbase64);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    LoginRequest json = new LoginRequest
                    {
                        Username = userName,
                        Password = password
                    };

                    reqeuest = JsonConvert.SerializeObject(json);

                    HttpContent content = new StringContent(JsonConvert.SerializeObject(json), Encoding.UTF8, "application/json");

                    var res = client.PostAsync(new Uri(url + "User/Login"), content).Result;

                    string statusCod = res.StatusCode.ToString();

                    response = res.Content.ReadAsStringAsync().Result;

                    loginDetails = JsonConvert.DeserializeObject<LoginObject>(res.Content.ReadAsStringAsync().Result);

                    if (statusCod == "OK" && loginDetails.STATUS == StaticVariables.COREBANKINGSUCCESSSTATUS)
                    {

                        message = StaticVariables.SUCCESSSTATUSMASSAGE;
                        worked = true;
                    }
                    else
                    {
                        message = StaticVariables.LOGINATTEMPTFAILED;

                    }
                }
            }
            catch (Exception ex)
            {
                message = StaticVariables.SERVERERRORMESSAGE;
                response = ex.Message + "||" + ex.StackTrace;
                Task.Factory.StartNew(() => WriteLog(logId.ToString(), string.Empty, ex.Message, StaticVariables.EXCEPTIONERROR, string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name)));
            }
            Task.Factory.StartNew(() => WriteLog(logId.ToString(), string.Empty, response, StaticVariables.COREBANKING, string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name)));
            return worked;
        }


        public static bool VerifyPointingAccountReference(int logId, string reference, string acountNumber, out decimal amount, out string remarks, out string message)
        {
            bool worked = false;
            amount = 0;
            remarks = string.Empty;
            message = string.Empty;

            try
            {

                using (FABMerchantPortalDBEntities db = new FABMerchantPortalDBEntities())
                {
                    var poinitngAccountUsed = db.PointingAccountReferences.FirstOrDefault(x => x.Reference.Equals(reference) && x.Status == true);

                    if (poinitngAccountUsed != null)
                    {
                        message = StaticVariables.DUPLICATEPOINTINGACCOUNT;
                        return worked;
                    }

                    if (CoreBankingPointingAccountReference(logId, reference, acountNumber, out PointingAccountObject coreBankingPoitingAccountrDetailsResponse, out message))
                    {
                        amount = coreBankingPoitingAccountrDetailsResponse.MESSAGE.TrxnAmount;
                        remarks = coreBankingPoitingAccountrDetailsResponse.MESSAGE.Description;
                        worked = true;
                    }


                }
            }
            catch (Exception ex)
            {

                Task.Factory.StartNew(() => WriteLog(logId.ToString(), reference, ex.Message, StaticVariables.EXCEPTIONERROR, string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name)));

            }

            return worked;
        }


        public static bool CoreBankingPointingAccountReference(int logId, string referenceNo, string accountNumber, out PointingAccountObject coreBankingPoitingAccountrDetailsResponse, out string message)
        {
            bool worked = false;

            message = StaticVariables.INVALIDPOINTINGACCOUNT;
            string reqeuest = string.Empty;
            string response = string.Empty;
            coreBankingPoitingAccountrDetailsResponse = null;
            string url = ConfigurationManager.AppSettings["CoreBankingBaseURL"];
            string merchantbase64 = ConfigurationManager.AppSettings["CoreBankingAuthorizationKey"];
            string coreBankingChannelId = ConfigurationManager.AppSettings["coreBankingChannelId"];
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", merchantbase64);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var json = new
                    {
                        referenceNo
                    };

                    reqeuest = JsonConvert.SerializeObject(json);


                    HttpContent content = new StringContent(JsonConvert.SerializeObject(json), Encoding.UTF8, "application/json");


                    var res = client.GetAsync(new Uri(url + "Transaction/GetTransactionDetail/" + coreBankingChannelId + "/" + accountNumber + "/" + referenceNo)).Result;

                    string statusCod = res.StatusCode.ToString();

                    response = res.Content.ReadAsStringAsync().Result;

                    //string updatedResponse = response.Replace("MESSAGE", "POINTINACCOUNTGMESSAGE");

                    coreBankingPoitingAccountrDetailsResponse = JsonConvert.DeserializeObject<PointingAccountObject>(response);

                    if (statusCod == "OK" && coreBankingPoitingAccountrDetailsResponse.STATUS == StaticVariables.COREBANKINGSUCCESSSTATUS)
                    {

                        message = StaticVariables.SUCCESSSTATUSMASSAGE;

                        worked = true;
                    }

                }
            }
            catch (Exception ex)
            {
                message = StaticVariables.SERVERERRORMESSAGE;
                response = ex.Message + "||" + ex.StackTrace;
                Task.Factory.StartNew(() => WriteLog(logId.ToString(), reqeuest, ex.Message, StaticVariables.EXCEPTIONERROR, string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name)));
            }
            Task.Factory.StartNew(() => WriteLog(logId.ToString(), reqeuest, response, StaticVariables.COREBANKING, string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name)));

            return worked;
        }


        public static void LogPointingAccountReference(int logId, string reference, string accountNumber, string user, bool status)
        {
          
            PointingAccountReference transfer = new PointingAccountReference();
            try
            {
                using (FABMerchantPortalDBEntities db = new FABMerchantPortalDBEntities())
                {
                    transfer = new PointingAccountReference
                    {
                        LogId = logId,
                        AccountNumber = accountNumber,
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                       
                        Reference = reference,
                        Status=status
                       
                    };
                    db.PointingAccountReferences.Add(transfer);
                    db.SaveChanges();
                  
                }

            }
            catch (Exception ex)
            {

                Task.Factory.StartNew(() => WriteLog(logId.ToString(), JsonConvert.SerializeObject(transfer), ex.Message, StaticVariables.EXCEPTIONERROR, string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name)));

            }

        }


     
    }
}