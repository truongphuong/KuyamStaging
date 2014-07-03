using Kuyam.Database;
using Kuyam.Domain;
using Kuyam.Domain.BlogServices;
using Kuyam.Domain.MessageServcies;
using Kuyam.Domain.SearchServices;
using Kuyam.Domain.SmsServices;
using Kuyam.Domain.Tasks;
using Kuyam.WebUI.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;


namespace Kuyam.WebUI.Controllers
{
    public class SmsAttController : KuyamBaseController
    {

        private readonly ISMSProvider _smsProvider;
        private readonly ISmsServices _smsServices;
        private readonly IBlogPostService _postService;
        private readonly ISearchService _searchService;
        public SmsAttController(ISMSProvider smsProvider,
            ISmsServices smsServices,
            IBlogPostService postService,
            ISearchService searchService)
        {
            this._smsProvider = smsProvider;
            this._smsServices = smsServices;
            this._postService = postService;
            this._searchService = searchService;
        }

        public ActionResult Index()
        {
            //Stopwatch stopwatch = new Stopwatch();
            int totalRecord = 0;
            var list = _searchService.SearchCompanies(out totalRecord);
            _postService.GetTest();
            //// Begin timing
            //stopwatch.Start();
            //var post = _postService.GetListPost(0, 0);
            //stopwatch.Stop();
            //ViewBag.Elapsed = stopwatch.Elapsed;    
            if (Request.Cookies.Count > 0)
            {

                foreach (string s in Request.Cookies.AllKeys)
                {
                    if (s == "KUYAM.SESSION")
                    {
                        Response.Cookies[s].Expires = DateTime.Now.AddYears(-1);

                    }
                }
            }
            return View();
        }


        public ActionResult GetMessageHeaders(string headerCount, string indexCursor)
        {
            int hCount = 100;

            string phoneinApp = ConfigurationManager.AppSettings["PhoneInapp"];
            var messageHeadersList = _smsProvider.GetMessageList(hCount.ToString(), "0");
            if (messageHeadersList == null)
                return Json("", JsonRequestBehavior.AllowGet);

            var query = _smsServices.GetAll(hCount);
            var newMessage = messageHeadersList.messages.Where(m => !query.Contains(m.messageId)).ToList();
            var custQuery = _smsServices.GetAllCust();
            int? custId = null;
            if (newMessage != null && newMessage.Count() > 0)
            {
                newMessage.Reverse();
                foreach (Message item in newMessage)
                {
                    char[] separators = new char[] { '+', '1' };
                    string from = item.from.value.TrimStart(separators);
                    string to = item.recipients[0].value.ToString().TrimStart(separators);
                    string formatPhonefrom = UtilityHelper.FormatPhone(from);
                    string formatPhoneTo = UtilityHelper.FormatPhone(from);

                    var custs = custQuery.Where(m => (m.MobilePhone == from || m.MobilePhone == formatPhonefrom) || (m.MobilePhone == to || m.MobilePhone == formatPhoneTo)).ToList();
                    if (custs != null && custs.Count() > 0)
                    {
                        foreach (var cust in custs)
                        {
                            custId = cust.CustID;
                            var message = new SMSMessage
                            {
                                MessageId = item.messageId,
                                Message = item.text,
                                SenderAddress = from,
                                DestinationAddress = to,
                                DateTime = DateTime.Parse(item.timeStamp),
                                CustId = custId,
                                DeliveryStatus = (int)Types.DeliveryStatus.Send,
                                ReceivedStatus = (int)Types.ReceiveStatus.Receive

                            };
                            _smsServices.AddMessage(message);
                        }
                    }
                    else
                    {
                        var message = new SMSMessage
                        {
                            MessageId = item.messageId,
                            Message = item.text,
                            SenderAddress = from,
                            DestinationAddress = to,
                            DateTime = DateTime.Parse(item.timeStamp),
                            CustId = null,
                            DeliveryStatus = (int)Types.DeliveryStatus.Send,
                            ReceivedStatus = (int)Types.ReceiveStatus.Receive

                        };
                        _smsServices.AddMessage(message);
                    }


                }

            }
            return Json(messageHeadersList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeliveryStatus()
        {
            Stream stream = Request.InputStream;

            if (null != stream)
            {
                byte[] bytes = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(bytes, 0, (int)stream.Length);
                string responseData = Encoding.ASCII.GetString(bytes);

                JavaScriptSerializer serializeObject = new JavaScriptSerializer();
                DeliveryStatusNotification message = (DeliveryStatusNotification)serializeObject.Deserialize(responseData, typeof(DeliveryStatusNotification));

                //if (null != message)
                //{

                //}
                EmailHelper.SendMail("Att@kuyam.com", "phuongtruong@vinasource.com", message.deliveryInfoNotification.deliveryInfo.address, message.deliveryInfoNotification.deliveryInfo.deliveryStatus);
            }

            return Json("succes", JsonRequestBehavior.AllowGet);
        }


        public ActionResult ReceiveStatus()
        {
            Stream stream = Request.InputStream;

            if (null != stream)
            {
                byte[] bytes = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(bytes, 0, (int)stream.Length);
                string responseData = Encoding.ASCII.GetString(bytes);

                JavaScriptSerializer serializeObject = new JavaScriptSerializer();
                InboundSMSMessage message = (InboundSMSMessage)serializeObject.Deserialize(responseData, typeof(InboundSMSMessage));

                if (null != message)
                {

                }
                EmailHelper.SendMail("Att@kuyam.com", "phuongtruong@vinasource.com", message.SenderAddress, message.Message);
            }

            return Json("succes", JsonRequestBehavior.AllowGet);
        }


    }



}
