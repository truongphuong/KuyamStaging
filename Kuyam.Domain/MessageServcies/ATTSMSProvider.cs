using Kuyam.Database;
using Kuyam.Domain.Authentication;
using Kuyam.Domain.Services;
using Kuyam.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace Kuyam.Domain.MessageServcies
{
    public class ATTSMSProvider : ISMSProvider
    {

        private int maxAddresses = 10;
        /// <summary>
        /// List of addresses to send
        /// </summary>
        private List<string> addressList = new List<string>();

        /// <summary>
        /// Variable to hold phone number(s)/email address(es)/short code(s) parameter.
        /// </summary>
        private string phoneNumbersParameter = null;

        private string AttachmentFilesDir = string.Empty;

        public List<string> attachments = null;
        public string sendMessageSuccessResponse = string.Empty;
        public string sendMessageErrorResponse = string.Empty;
        public string getHeadersErrorResponse = string.Empty;

        public string getHeadersSuccessResponse = string.Empty;
        public string getMessageSuccessResponse = string.Empty;

        public string getMessageErrorResponse = string.Empty;
        public string content_result = string.Empty;

        public byte[] receivedBytes = null;
        public WebResponse getContentResponseObject = null;
        public string[] imageData = null;
        public MessageHeaderList messageHeaderList;

        public string getMessageListErrorResponse = string.Empty;
             
        public MessageList GetMessageList(string limit, string offset)
        {
            try
            {
                var authentication = new AuthenticationAT(new SettingService());
                if (authentication.ReadAndGetAccessToken(AccessTokenType.Authorization_Code, ref sendMessageErrorResponse))
                {
                    string contextURL = authentication.EndPoint + "/myMessages/v2/messages?limit=" + limit + "&offset=" + offset;

                    HttpWebRequest getMessageListWebRequest = (HttpWebRequest)WebRequest.Create(contextURL);
                    getMessageListWebRequest.Headers.Add("Authorization", "Bearer " + authentication.AccessToken);
                    getMessageListWebRequest.Method = "GET";
                    getMessageListWebRequest.KeepAlive = true;
                    WebResponse getMessageListWebResponse = getMessageListWebRequest.GetResponse();
                    using (var stream = getMessageListWebResponse.GetResponseStream())
                    {
                        StreamReader sr = new StreamReader(stream);
                        string csGetMessageListDetailsData = sr.ReadToEnd();

                        JavaScriptSerializer deserializeJsonObject = new JavaScriptSerializer();
                        csGetMessageListDetails deserializedJsonObj = (csGetMessageListDetails)deserializeJsonObject.Deserialize(csGetMessageListDetailsData, typeof(csGetMessageListDetails));

                        if (null != deserializedJsonObj)
                        {
                            return deserializedJsonObj.messageList;
                        }
                        sr.Close();
                    }
                }
            }
            catch (WebException we)
            {
                string errorResponse = string.Empty;
                try
                {
                    using (StreamReader sr2 = new StreamReader(we.Response.GetResponseStream()))
                    {
                        errorResponse = sr2.ReadToEnd();
                        sr2.Close();
                    }
                    getMessageListErrorResponse = errorResponse;
                }
                catch
                {
                    errorResponse = "Unable to get response";
                    getMessageListErrorResponse = errorResponse;
                }
            }
            catch (Exception ex)
            {
                getMessageListErrorResponse = ex.Message;
                return null;
            }
            return null;
        }

        public void CreateMessageIndex()
        {
            try
            {
                var authentication = new AuthenticationAT(new SettingService());
                if (authentication.ReadAndGetAccessToken(AccessTokenType.Authorization_Code, ref sendMessageErrorResponse))
                {
                    HttpWebRequest createMessageIndexWebRequest = (HttpWebRequest)WebRequest.Create(authentication.EndPoint + "/myMessages/v2/messages/index");
                    createMessageIndexWebRequest.Headers.Add("Authorization", "Bearer " + authentication.AccessToken);
                    createMessageIndexWebRequest.Method = "POST";
                    createMessageIndexWebRequest.KeepAlive = true;
                    UTF8Encoding encoding = new UTF8Encoding();
                    byte[] postBytes = encoding.GetBytes("");
                    //createMessageIndexWebRequest.ContentLength = postBytes.Length;
                    Stream postStream = createMessageIndexWebRequest.GetRequestStream();
                    postStream.Write(postBytes, 0, postBytes.Length);
                    postStream.Close();
                    WebResponse createMessageIndexWebResponse = createMessageIndexWebRequest.GetResponse();
                    using (var stream = createMessageIndexWebResponse.GetResponseStream())
                    {

                    }
                }
            }
            catch (WebException we)
            {
                string errorResponse = string.Empty;
                try
                {
                    using (StreamReader sr2 = new StreamReader(we.Response.GetResponseStream()))
                    {
                        errorResponse = sr2.ReadToEnd();
                        sr2.Close();
                    }

                }
                catch
                {
                    errorResponse = "Unable to get response";

                }
            }
            catch (Exception ex)
            {
                return;
            }

        }
        public MessageHeaderList GetMessageHeaders(string hCount, string iCursor)
        {
            try
            {
                HttpWebRequest mimRequestObject;
                var authentication = new AuthenticationAT(new SettingService());

                if (authentication.ReadAndGetAccessToken(AccessTokenType.Authorization_Code, ref sendMessageErrorResponse))
                {
                    string getHeadersURL = authentication.EndPoint + "/rest/1/MyMessages?HeaderCount=" + hCount;
                    if (!string.IsNullOrEmpty(iCursor))
                    {
                        getHeadersURL += "&IndexCursor=" + iCursor;
                    }
                    mimRequestObject = (HttpWebRequest)WebRequest.Create(getHeadersURL);
                    mimRequestObject.Headers.Add("Authorization", "Bearer " + authentication.AccessToken);
                    mimRequestObject.Method = "GET";
                    mimRequestObject.KeepAlive = true;

                    WebResponse mimResponseObject1 = mimRequestObject.GetResponse();
                    using (StreamReader sr = new StreamReader(mimResponseObject1.GetResponseStream()))
                    {
                        string mimResponseData = sr.ReadToEnd();

                        JavaScriptSerializer deserializeJsonObject = new JavaScriptSerializer();
                        MIMResponse deserializedJsonObj = (MIMResponse)deserializeJsonObject.Deserialize(mimResponseData, typeof(MIMResponse));

                        if (deserializedJsonObj != null)
                        {
                            getHeadersSuccessResponse = "Success";
                            return deserializedJsonObj.MessageHeadersList;
                        }
                        sr.Close();
                        getHeadersErrorResponse = "No response from server";
                        return null;
                    }
                }

                return null;
            }
            catch (WebException we)
            {
                string errorResponse = string.Empty;

                try
                {
                    using (StreamReader sr2 = new StreamReader(we.Response.GetResponseStream()))
                    {
                        errorResponse = sr2.ReadToEnd();
                        sr2.Close();
                    }
                    getHeadersErrorResponse = errorResponse;
                }
                catch
                {
                    errorResponse = "Unable to get response";
                    getHeadersErrorResponse = errorResponse;
                }
            }
            catch (Exception ex)
            {
                getHeadersErrorResponse = ex.Message;
                return null;
            }

            return null;
        }

        public SendSMSResult SendSms(string[] address, string subject, string message, bool groupflag, ArrayList attachments = null)
        {
            if (!this.IsValidAddress(address, groupflag))
            {
                //NotificationErrorZendeskTicket("send error", sendMessageErrorResponse, string.Join("", address));
                return new SendSMSResult(EResultStatus.Fail, sendMessageErrorResponse);
            }

            var authentication = new AuthenticationAT(new SettingService());

            if (authentication.ReadAndGetAccessToken(AccessTokenType.Authorization_Code, ref sendMessageErrorResponse) == true)
            {
                string accessToken = authentication.AccessToken;
                string endpoint = authentication.EndPoint;
                this.SendMessageRequest(accessToken, endpoint, subject, message, groupflag, attachments);
            }

            if (!string.IsNullOrEmpty(sendMessageErrorResponse))
            {
                //NotificationErrorZendeskTicket("send error", sendMessageErrorResponse, string.Join("", address));
                return new SendSMSResult(EResultStatus.Fail, sendMessageErrorResponse);
            }
            return new SendSMSResult(EResultStatus.Success, "Success", sendMessageSuccessResponse);
        }

        public void NotificationErrorZendeskTicket(string subject, string message, string phoneNumber = "")
        {
            string url = string.Empty;
            if (ConfigurationManager.AppSettings["KuyamVersion"] != null && int.Parse(ConfigurationManager.AppSettings["KuyamVersion"]) == (int)Types.KuyamVersion.QA)
            {
                subject = "[QA] " + subject;
                url = ConfigurationManager.AppSettings["WebHost"];
            }
            else if (ConfigurationManager.AppSettings["KuyamVersion"] != null && int.Parse(ConfigurationManager.AppSettings["KuyamVersion"]) == (int)Types.KuyamVersion.Dev)
            {
                subject = "[DEV] " + subject;
                url = ConfigurationManager.AppSettings["WebHost"];
            }

            var status = (TicketStatus.Pending);
            var type = (TicketType.Incident);
            var priority = (TicketPriority.High);

            string description = phoneNumber + ":" + message;

            ZenAPI.CreateTicket(subject, status, type, priority, description);
        }

        private bool IsValidAddress(string[] addresses, bool groupflag)
        {
            string phonenumbers = string.Empty;

            bool isValid = true;
            if (addresses == null)
            {
                sendMessageErrorResponse = "Address field cannot be blank.";
                return false;
            }

            if (addresses.Length > this.maxAddresses)
            {
                sendMessageErrorResponse = "Message cannot be delivered to more than 10 receipients.";
                return false;
            }

            if (groupflag && addresses.Length < 2)
            {
                sendMessageErrorResponse = "Specify more than one address for Group message.";
                return false;
            }

            foreach (string addressraw in addresses)
            {
                string address = addressraw.Trim();
                if (string.IsNullOrEmpty(address))
                {
                    break;
                }

                if (address.Length < 3)
                {
                    sendMessageErrorResponse = "Invalid address specified.";
                    return false;
                }

                // Verify if short codes are present in address
                if (!address.StartsWith("short") && (address.Length > 2 && address.Length < 9))
                {
                    if (groupflag)
                    {
                        sendMessageErrorResponse = "Group Message with short codes is not allowed.";
                        return false;
                    }

                    this.addressList.Add(address);
                    this.phoneNumbersParameter = this.phoneNumbersParameter + "Addresses=short:" + HttpContext.Current.Server.UrlEncode(address.ToString()) + "&";
                }

                if (address.StartsWith("short"))
                {
                    if (groupflag)
                    {
                        sendMessageErrorResponse = "Group Message with short codes is not allowed.";
                        return false;
                    }

                    Regex regex = new Regex("^[0-9]*$");
                    if (!regex.IsMatch(address.Substring(6)))
                    {
                        sendMessageErrorResponse = "Invalid short code specified.";
                        return false;
                    }

                    this.addressList.Add(address);
                    this.phoneNumbersParameter = this.phoneNumbersParameter + "Addresses=" + HttpContext.Current.Server.UrlEncode(address.ToString()) + "&";
                }
                else if (address.Contains("@"))
                {
                    isValid = this.IsValidEmail(address);
                    if (isValid == false)
                    {
                        sendMessageErrorResponse = "Specified Email Address is invalid.";
                        return false;
                    }
                    else
                    {
                        this.addressList.Add(address);
                        this.phoneNumbersParameter = this.phoneNumbersParameter + "Addresses=" + HttpContext.Current.Server.UrlEncode(address.ToString()) + "&";
                    }
                }
                else
                {
                    if (this.IsValidMISDN(address) == true)
                    {
                        if (address.StartsWith("tel:"))
                        {
                            phonenumbers = address.Replace("-", string.Empty);
                            this.phoneNumbersParameter = this.phoneNumbersParameter + "Addresses=" + HttpContext.Current.Server.UrlEncode(phonenumbers.ToString()) + "&";
                        }
                        else
                        {
                            phonenumbers = address.Replace("-", string.Empty);
                            this.phoneNumbersParameter = this.phoneNumbersParameter + "Addresses=" + HttpContext.Current.Server.UrlEncode("tel:" + phonenumbers.ToString()) + "&";
                        }

                        this.addressList.Add(address);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Validate given string for MSISDN
        /// </summary>
        /// <param name="number">Phone number to be validated</param>
        /// <returns>true/false; true - if valid MSISDN, else false</returns>
        private bool IsValidMISDN(string number)
        {
            string smsAddressInput = number;
            long tryParseResult = 0;
            string smsAddressFormatted;
            string phoneStringPattern = "^\\d{3}-\\d{3}-\\d{4}$";
            if (Regex.IsMatch(smsAddressInput, phoneStringPattern))
            {
                smsAddressFormatted = smsAddressInput.Replace("-", string.Empty);
            }
            else
            {
                smsAddressFormatted = smsAddressInput;
            }

            if (smsAddressFormatted.Length == 16 && smsAddressFormatted.StartsWith("tel:+1"))
            {
                smsAddressFormatted = smsAddressFormatted.Substring(6, 10);
            }
            else if (smsAddressFormatted.Length == 15 && smsAddressFormatted.StartsWith("tel:1"))
            {
                smsAddressFormatted = smsAddressFormatted.Substring(5, 10);
            }
            else if (smsAddressFormatted.Length == 14 && smsAddressFormatted.StartsWith("tel:"))
            {
                smsAddressFormatted = smsAddressFormatted.Substring(4, 10);
            }
            else if (smsAddressFormatted.Length == 12 && smsAddressFormatted.StartsWith("+1"))
            {
                smsAddressFormatted = smsAddressFormatted.Substring(2, 10);
            }
            else if (smsAddressFormatted.Length == 11 && smsAddressFormatted.StartsWith("1"))
            {
                smsAddressFormatted = smsAddressFormatted.Substring(1, 10);
            }

            if ((smsAddressFormatted.Length != 10) || (!long.TryParse(smsAddressFormatted, out tryParseResult)))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates given mail ID for standard mail format
        /// </summary>
        /// <param name="emailID">Mail Id to be validated</param>
        /// <returns> true/false; true - if valid email id, else false</returns>
        private bool IsValidEmail(string emailID)
        {
            string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                  @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                  @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            Regex re = new Regex(strRegex);
            if (re.IsMatch(emailID))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Validates a given string for digits
        /// </summary>
        /// <param name="address">string to be validated</param>
        /// <returns>true/false; true - if passed string has all digits, else false</returns>
        private bool IsNumber(string address)
        {
            bool isValid = false;
            Regex regex = new Regex("^[0-9]*$");
            if (regex.IsMatch(address))
            {
                isValid = true;
            }

            return isValid;
        }

        /// <summary>
        /// Sends message to the list of addresses provided.
        /// </summary>
        /// <param name="attachments">List of attachments</param>
        private void SendMessageRequest(string accToken, string edPoint, string subject, string message, bool groupflag, ArrayList attachments)
        {
            
            Stream postStream = null;
            try
            {
                string boundaryToSend = "----------------------------" + DateTime.Now.Ticks.ToString("x");

                HttpWebRequest msgRequestObject = (HttpWebRequest)WebRequest.Create(string.Empty + edPoint + "/rest/1/MyMessages");
                msgRequestObject.Headers.Add("Authorization", "Bearer " + accToken);
                msgRequestObject.Method = "POST";
                string contentType = "multipart/form-data; type=\"application/x-www-form-urlencoded\"; start=\"<startpart>\"; boundary=\"" + boundaryToSend + "\"\r\n";
                msgRequestObject.ContentType = contentType;
                string mmsParameters = this.phoneNumbersParameter + "Subject=" + HttpContext.Current.Server.UrlEncode(subject) + "&Text=" + HttpContext.Current.Server.UrlEncode(message) + "&Group=" + groupflag.ToString().ToLower();

                string dataToSend = string.Empty;
                dataToSend += "--" + boundaryToSend + "\r\n";
                dataToSend += "Content-Type: application/x-www-form-urlencoded; charset=UTF-8\r\nContent-Transfer-Encoding: 8bit\r\nContent-Disposition: form-data; name=\"root-fields\"\r\nContent-ID: <startpart>\r\n\r\n" + mmsParameters + "\r\n";

                UTF8Encoding encoding = new UTF8Encoding();
                if ((attachments == null) || (attachments.Count == 0))
                {
                    if (!groupflag)
                    {
                        msgRequestObject.ContentType = "application/x-www-form-urlencoded";
                        byte[] postBytes = encoding.GetBytes(mmsParameters);
                        msgRequestObject.ContentLength = postBytes.Length;

                        postStream = msgRequestObject.GetRequestStream();
                        postStream.Write(postBytes, 0, postBytes.Length);
                        postStream.Close();

                        WebResponse mmsResponseObject1 = msgRequestObject.GetResponse();
                        using (StreamReader sr = new StreamReader(mmsResponseObject1.GetResponseStream()))
                        {
                            string mmsResponseData = sr.ReadToEnd();
                            JavaScriptSerializer deserializeJsonObject = new JavaScriptSerializer();
                            MsgResponseId deserializedJsonObj = (MsgResponseId)deserializeJsonObject.Deserialize(mmsResponseData, typeof(MsgResponseId));
                            if (deserializedJsonObj.Id != null)
                            {
                                sendMessageSuccessResponse = deserializedJsonObj.Id.ToString();
                            }
                            else
                            {
                                sendMessageSuccessResponse = "Cannot get response message from AT&T";
                            }
                            sr.Close();
                        }
                    }
                    else
                    {
                        dataToSend += "--" + boundaryToSend + "--\r\n";
                        byte[] bytesToSend = encoding.GetBytes(dataToSend);

                        int sizeToSend = bytesToSend.Length;

                        var memBufToSend = new MemoryStream(new byte[sizeToSend], 0, sizeToSend, true, true);
                        memBufToSend.Write(bytesToSend, 0, bytesToSend.Length);

                        byte[] finalData = memBufToSend.GetBuffer();
                        msgRequestObject.ContentLength = finalData.Length;

                        postStream = msgRequestObject.GetRequestStream();
                        postStream.Write(finalData, 0, finalData.Length);

                        WebResponse mmsResponseObject1 = msgRequestObject.GetResponse();
                        using (StreamReader sr = new StreamReader(mmsResponseObject1.GetResponseStream()))
                        {
                            string mmsResponseData = sr.ReadToEnd();
                            JavaScriptSerializer deserializeJsonObject = new JavaScriptSerializer();
                            MsgResponseId deserializedJsonObj = (MsgResponseId)deserializeJsonObject.Deserialize(mmsResponseData, typeof(MsgResponseId));
                            if (deserializedJsonObj.Id != null)
                            {
                                sendMessageSuccessResponse = deserializedJsonObj.Id.ToString();
                            }
                            else
                            {
                                sendMessageSuccessResponse = "Cannot get response message from AT&T";
                            }
                            
                            sr.Close();
                        }
                    }
                }
                else
                {
                    byte[] dataBytes = encoding.GetBytes(string.Empty);
                    byte[] totalDataBytes = encoding.GetBytes(string.Empty);
                    int count = 0;
                    foreach (string attachment in attachments)
                    {
                        string mmsFileName = Path.GetFileName(attachment.ToString());
                        string mmsFileExtension = Path.GetExtension(attachment.ToString());
                        string attachmentContentType = this.GetContentTypeFromExtension(mmsFileExtension);
                        FileStream imageFileStream = new FileStream(attachment.ToString(), FileMode.Open, FileAccess.Read);
                        BinaryReader imageBinaryReader = new BinaryReader(imageFileStream);
                        byte[] image = imageBinaryReader.ReadBytes((int)imageFileStream.Length);
                        imageBinaryReader.Close();
                        imageFileStream.Close();
                        if (count == 0)
                        {
                            dataToSend += "\r\n--" + boundaryToSend + "\r\n";
                        }
                        else
                        {
                            dataToSend = "\r\n--" + boundaryToSend + "\r\n";
                        }

                        dataToSend += "Content-Disposition: form-data; name=\"file" + count + "\"; filename=\"" + mmsFileName + "\"\r\n";
                        dataToSend += "Content-Type:" + attachmentContentType + "\r\n";
                        dataToSend += "Content-ID:<" + mmsFileName + ">\r\n";
                        dataToSend += "Content-Transfer-Encoding:binary\r\n\r\n";
                        byte[] dataToSendByte = encoding.GetBytes(dataToSend);
                        int dataToSendSize = dataToSendByte.Length + image.Length;
                        var tempMemoryStream = new MemoryStream(new byte[dataToSendSize], 0, dataToSendSize, true, true);
                        tempMemoryStream.Write(dataToSendByte, 0, dataToSendByte.Length);
                        tempMemoryStream.Write(image, 0, image.Length);
                        dataBytes = tempMemoryStream.GetBuffer();
                        if (count == 0)
                        {
                            totalDataBytes = dataBytes;
                        }
                        else
                        {
                            byte[] tempForTotalBytes = totalDataBytes;
                            var tempMemoryStreamAttach = this.JoinTwoByteArrays(tempForTotalBytes, dataBytes);
                            totalDataBytes = tempMemoryStreamAttach.GetBuffer();
                        }

                        count++;
                    }

                    byte[] byteLastBoundary = encoding.GetBytes("\r\n--" + boundaryToSend + "--\r\n");
                    int totalDataSize = totalDataBytes.Length + byteLastBoundary.Length;
                    var totalMemoryStream = new MemoryStream(new byte[totalDataSize], 0, totalDataSize, true, true);
                    totalMemoryStream.Write(totalDataBytes, 0, totalDataBytes.Length);
                    totalMemoryStream.Write(byteLastBoundary, 0, byteLastBoundary.Length);
                    byte[] finalpostBytes = totalMemoryStream.GetBuffer();

                    msgRequestObject.ContentLength = finalpostBytes.Length;

                    postStream = msgRequestObject.GetRequestStream();
                    postStream.Write(finalpostBytes, 0, finalpostBytes.Length);

                    WebResponse mmsResponseObject1 = msgRequestObject.GetResponse();
                    using (StreamReader sr = new StreamReader(mmsResponseObject1.GetResponseStream()))
                    {
                        string mmsResponseData = sr.ReadToEnd();
                        JavaScriptSerializer deserializeJsonObject = new JavaScriptSerializer();
                        MsgResponseId deserializedJsonObj = (MsgResponseId)deserializeJsonObject.Deserialize(mmsResponseData, typeof(MsgResponseId));
                        if (deserializedJsonObj.Id != null)
                        {
                            sendMessageSuccessResponse = deserializedJsonObj.Id.ToString();
                        }
                        else
                        {
                            sendMessageSuccessResponse = "Cannot get response message from AT&T";
                        }
                        sr.Close();
                    }
                }



            }
            catch (WebException we)
            {
                if (null != we.Response)
                {
                    using (Stream stream = we.Response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        sendMessageErrorResponse = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                sendMessageErrorResponse = ex.ToString();
            }
            finally
            {
                if (null != postStream)
                {
                    postStream.Close();
                }
            }


        }


        /// <summary>
        /// Gets the mapping of extension with predefined content types
        /// </summary>
        /// <param name="extension">file extension</param>
        /// <returns>string, content type</returns>
        private string GetContentTypeFromExtension(string extension)
        {
            Dictionary<string, string> extensionToContentType = new Dictionary<string, string>()
            {
                { ".jpg", "image/jpeg" }, { ".bmp", "image/bmp" }, { ".mp3", "audio/mp3" },
                { ".m4a", "audio/m4a" }, { ".gif", "image/gif" }, { ".3gp", "video/3gpp" },
                { ".3g2", "video/3gpp2" }, { ".wmv", "video/x-ms-wmv" }, { ".m4v", "video/x-m4v" },
                { ".amr", "audio/amr" }, { ".mp4", "video/mp4" }, { ".avi", "video/x-msvideo" },
                { ".mov", "video/quicktime" }, { ".mpeg", "video/mpeg" }, { ".wav", "audio/x-wav" },
                { ".aiff", "audio/x-aiff" }, { ".aifc", "audio/x-aifc" }, { ".midi", ".midi" },
                { ".au", "audio/basic" }, { ".xwd", "image/x-xwindowdump" }, { ".png", "image/png" },
                { ".tiff", "image/tiff" }, { ".ief", "image/ief" }, { ".txt", "text/plain" },
                { ".html", "text/html" }, { ".vcf", "text/x-vcard" }, { ".vcs", "text/x-vcalendar" },
                { ".mid", "application/x-midi" }, { ".imy", "audio/iMelody" }
            };
            if (extensionToContentType.ContainsKey(extension))
            {
                return extensionToContentType[extension];
            }
            else
            {
                return "Not Found";
            }
        }


        /// <summary>
        /// Sums up two byte arrays.
        /// </summary>
        /// <param name="firstByteArray">First byte array</param>
        /// <param name="secondByteArray">second byte array</param>
        /// <returns>The memorystream"/> summed memory stream</returns>
        private MemoryStream JoinTwoByteArrays(byte[] firstByteArray, byte[] secondByteArray)
        {
            int newSize = firstByteArray.Length + secondByteArray.Length;
            var totalMemoryStream = new MemoryStream(new byte[newSize], 0, newSize, true, true);
            totalMemoryStream.Write(firstByteArray, 0, firstByteArray.Length);
            totalMemoryStream.Write(secondByteArray, 0, secondByteArray.Length);
            return totalMemoryStream;
        }


    }


    /// <summary>
    /// Response returned from MyMessages api
    /// </summary>
    public class MIMResponse
    {
        /// <summary>
        /// Gets or sets the value of message header list.
        /// </summary>
        public MessageHeaderList MessageHeadersList
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Response from IMMN api
    /// </summary>
    public class MsgResponseId
    {
        /// <summary>
        /// Gets or sets Message ID
        /// </summary>
        public string Id { get; set; }
    }

    public class csGetMessageListDetails
    {
        public MessageList messageList { get; set; }
    }

    #region message list


    public class MessageList
    {
        public List<Message> messages { get; set; }
        public int offset { get; set; }
        public int limit { get; set; }
        public int total { get; set; }
        public string state { get; set; }
        public string cacheStatus { get; set; }
        public List<string> failedMessages { get; set; }
    }

    public class Message
    {
        public string messageId { get; set; }
        public From from { get; set; }
        public List<Recipient> recipients { get; set; }
        public string timeStamp { get; set; }
        public Boolean isFavorite { get; set; }
        public Boolean isUnread { get; set; }
        public string type { get; set; }
        public TypeMetaData typeMetaData { get; set; }
        public string isIncoming { get; set; }
        public List<MmsContent> mmsContent { get; set; }
        public string text { get; set; }
        public string subject { get; set; }
    }

    public class From
    {
        public string value { get; set; }
    }

    public class Recipient
    {
        public string value { get; set; }
    }

    public class TypeMetaData
    {
        public bool isSegmented { get; set; }
        public SegmentationDetails segmentationDetails { get; set; }
        public string subject { get; set; }
    }

    public class SegmentationDetails
    {

        public int segmentationMsgRefNumber { get; set; }

        public int totalNumberOfParts { get; set; }

        public int thisPartNumber { get; set; }

    }
    public class MmsContent
    {
        public string contentType { get; set; }
        public string contentName { get; set; }
        public string contentUrl { get; set; }
        public string type { get; set; }
    }

    #endregion  message list

    /// <summary>
    /// Message Header List
    /// </summary>
    public class MessageHeaderList
    {
        /// <summary>
        /// Gets or sets the value of object containing a List of Messages Headers
        /// </summary>
        public List<Header> Headers
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value of a number representing the number of headers returned for this request.
        /// </summary>
        public int HeaderCount
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value of a string which defines the start of the next block of messages for the current request.
        /// A value of zero (0) indicates the end of the block.
        /// </summary>
        public string IndexCursor
        {
            get;
            set;
        }
    }
    /// <summary>
    /// Object containing a List of Messages Headers
    /// </summary>
    public class Header
    {
        /// <summary>
        /// Gets or sets the value of Unique message identifier
        /// </summary>
        public string MessageId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value of message sender
        /// </summary>
        public string From
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value of the addresses, whom the message need to be delivered. 
        /// If Group Message, this will contain multiple Addresses.
        /// </summary>
        public List<string> To
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value of message text
        /// </summary>
        public string Text
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value of message part descriptions
        /// </summary>
        public List<MMSContent> MmsContent
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value of date/time message received
        /// </summary>
        public DateTime Received
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether its a favourite or not
        /// </summary>
        public bool Favorite
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether message is read or not
        /// </summary>
        public bool Read
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value of type of message, TEXT or MMS
        /// </summary>
        public string Type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value of indicator, which indicates if message is Incoming or Outgoing “IN” or “OUT”
        /// </summary>
        public string Direction
        {
            get;
            set;
        }
    }
    /// <summary>
    /// Message part descriptions
    /// </summary>
    public class MMSContent
    {
        /// <summary>
        /// Gets or sets the value of content name
        /// </summary>
        public string ContentName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value of content type
        /// </summary>
        public string ContentType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value of part number
        /// </summary>
        public string PartNumber
        {
            get;
            set;
        }
    }
    #region Data structure for send sms response

    ///<summary>
    ///Class to hold ResourceReference
    ///</summary>
    public class ResourceReference
    {
        ///<summary>
        ///Gets or sets resourceURL
        ///</summary>
        public string resourceURL { get; set; }
    }

    public class SendSMSResponse
    {
        public OutBoundSMSResponse outBoundSMSResponse;
    }
    /// <summary>
    /// Class to hold send sms response
    /// </summary>
    public class OutBoundSMSResponse
    {
        /// <summary>
        /// Gets or sets messageId
        /// </summary>
        public string messageId { get; set; }
        /// <summary>
        /// Gets or sets ResourceReference
        /// </summary>
        public ResourceReference resourceReference { get; set; }
    }

    public class SendSMSDataForSingle
    {
        public OutboundSMSRequestForSingle outboundSMSRequest { get; set; }
    }

    public class OutboundSMSRequestForSingle
    {
        /// <summary>
        /// Gets or sets the address to send.
        /// </summary>
        public string address
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets message text to send.
        /// </summary>
        public string message
        {
            get;
            set;
        }

        public bool notifyDeliveryStatus
        {
            get;
            set;
        }
    }
    public class SendSMSDataForMultiple
    {
        public OutboundSMSRequestForMultiple outboundSMSRequest { get; set; }
    }

    public class OutboundSMSRequestForMultiple
    {
        /// <summary>
        /// Gets or sets the list of address to send.
        /// </summary>
        public List<string> address
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets message text to send.
        /// </summary>
        public string message
        {
            get;
            set;
        }

        public bool notifyDeliveryStatus
        {
            get;
            set;
        }
    }
    #endregion
    #region Data structure for Get SMS (offline)
    /// <summary>
    /// Class to hold rececive sms response
    /// </summary>
    public class GetSmsResponse
    {
        /// <summary>
        /// Gets or sets inbound sms message list
        /// </summary>
        public InboundSMSMessageList InboundSMSMessageList { get; set; }
    }

    /// <summary>
    /// Class to hold inbound sms message list
    /// </summary>
    public class InboundSMSMessageList
    {
        /// <summary>
        /// Gets or sets inbound sms message
        /// </summary>
        public List<InboundSMSMessage> InboundSMSMessage { get; set; }

        /// <summary>
        /// Gets or sets number of messages in a batch
        /// </summary>
        public int NumberOfMessagesInThisBatch { get; set; }

        /// <summary>
        /// Gets or sets resource url
        /// </summary>
        public string ResourceURL { get; set; }

        /// <summary>
        /// Gets or sets total number of pending messages
        /// </summary>
        public int TotalNumberOfPendingMessages { get; set; }
    }

    /// <summary>
    /// Class to hold inbound sms message
    /// </summary>
    public class InboundSMSMessage
    {
        /// <summary>
        /// Gets or sets message id
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// Gets or sets message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets sender address
        /// </summary>
        public string SenderAddress { get; set; }
    }
    #endregion
    #region Data structure for Receive SMS (online)
    /// <summary>
    /// Class to hold inbound sms message
    /// </summary>
    public class ReceiveSMS
    {
        /// <summary>
        /// Gets or sets datetime
        /// </summary>
        public string DateTime { get; set; }

        /// <summary>
        /// Gets or sets destination address
        /// </summary>
        public string DestinationAddress { get; set; }

        /// <summary>
        /// Gets or sets message id
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// Gets or sets message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets sender address
        /// </summary>
        public string SenderAddress { get; set; }
    }
    #endregion
    #region Data structure for Get Delivery Status (offline)

    /// <summary>
    /// Class to hold delivery status
    /// </summary>
    public class GetDeliveryStatus
    {
        /// <summary>
        /// Gets or sets delivery info list
        /// </summary>
        public DeliveryInfoList DeliveryInfoList { get; set; }
    }

    /// <summary>
    /// Class to hold delivery info list
    /// </summary>
    public class DeliveryInfoList
    {
        /// <summary>
        /// Gets or sets resource url
        /// </summary>
        public string ResourceURL { get; set; }

        /// <summary>
        /// Gets or sets delivery info
        /// </summary>
        public List<DeliveryInfo> DeliveryInfo { get; set; }
    }

    /// <summary>
    /// Class to hold delivery info
    /// </summary>
    public class DeliveryInfo
    {
        /// <summary>
        /// Gets or sets id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets address
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets delivery status
        /// </summary>
        public string Deliverystatus { get; set; }
    }

    #endregion
    #region Data structure for receive delivery status

    public class ReceiveDeliveryInfo
    {
        /// <summary>
        /// Gets or sets the list of address.
        /// </summary>
        public string address
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets the list of deliveryStatus.
        /// </summary>
        public string deliveryStatus
        {
            get;
            set;
        }
    }
    public class DeliveryInfoNotification
    {
        /// <summary>
        /// Gets or sets the list of messageId.
        /// </summary>
        public string messageId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets message text to send.
        /// </summary>
        public ReceiveDeliveryInfo deliveryInfo
        {
            get;
            set;
        }
    }
    public class DeliveryStatusNotification
    {
        /// <summary>
        /// Gets or sets the list of messageId.
        /// </summary>
        public DeliveryInfoNotification deliveryInfoNotification
        {
            get;
            set;
        }
    }

    #endregion

}
