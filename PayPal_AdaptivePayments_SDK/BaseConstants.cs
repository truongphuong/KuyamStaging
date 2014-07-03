using System.Text;

namespace PayPal
{
	/// <summary>
	/// Summary description for Constants.
	/// </summary>
	public class BaseConstants
	{
        public const string XPAYPALREQUESTDATAFORMAT = "X-PAYPAL-REQUEST-DATA-FORMAT";
        public const string XPAYPALRESPONSEDATAFORMAT = "X-PAYPAL-RESPONSE-DATA-FORMAT";
        public const string XPAYPALSERVICEVERSION = "X-PAYPAL-SERVICE-VERSION";        
        public const string XPAYPALSECURITYUSERID = "X-PAYPAL-SECURITY-USERID";
        public const string XPAYPALSECURITYOAUTHSIGN = "X-PAYPAL-AUTHORIZATION";
        public const string XPAYPALSECURITYCLIENTCERT = "CLIENT-AUTH";
        public const string XPAYPALSECURITYPASSWORD = "X-PAYPAL-SECURITY-PASSWORD";
        public const string XPAYPALSECURITYSIGNATURE = "X-PAYPAL-SECURITY-SIGNATURE";
        public const string XPAYPALMESSAGEPROTOCOL = "X-PAYPAL-MESSAGE-PROTOCOL";
        public const string XPAYPALAPPLICATIONID = "X-PAYPAL-APPLICATION-ID";
        public const string XPAYPALDEVICEIPADDRESS = "X-PAYPAL-DEVICE-IPADDRESS";
        public const string XPAYPALSANDBOXEMAILADDRESS = "X-PAYPAL-SANDBOX-EMAIL-ADDRESS";
        public const string XPAYPALREQUESTSOURCE = "X-PAYPAL-REQUEST-SOURCE";        

        //Data Request format specified here
        public const string  RequestDataformat="NV";
        public const string  ResponseDataformat="NV";

        public const string REQUESTMETHOD = "POST";
        public const string PAYPALLOGFILE = "PAYPALLOGFILE";

        // Default connection timeout in milliseconds
        public const int DEFAULT_TIMEOUT = 3600000;

        // Encoding format to be used for API payloads
        public static readonly Encoding ENCODING_FORMAT = Encoding.UTF8;

        public const string SDK_NAME = "sdk-adaptivepayments-dotnet";
        public const string SDK_VERSION = "1.0.92";

        public class ErrorMessages
        {
            public const string PROFILE_NULL = "APIProfile cannot be null." ;
            public const string PAYLOAD_NULL = "PayLoad cannot be null or empty.";

            public const string err_endpoint = "Endpoint cannot be empty.";
            public const string err_username = "API username cannot be empty";
            public const string err_passeword = "API password cannot be empty.";
            public const string err_signature = "API signature cannot be empty";
            public const string err_appid = "Application Id cannot be empty";
            public const string err_certificate = "Certificate cannot be empty";
            public const string err_privatekeypassword = "Private Key password cannot be null or empty.";
        }
	}
}
