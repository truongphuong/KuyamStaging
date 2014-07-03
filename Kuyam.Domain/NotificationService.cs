using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Kuyam.Database;
using Kuyam.Repository.Interface;
using Kuyam.Utility;
using Newtonsoft.Json.Linq;
using PushSharp;
using PushSharp.Android;
using PushSharp.Apple;
using PushSharp.Core;

namespace Kuyam.Domain
{
    public class NotificationService
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationService" /> class.
        /// </summary>
        /// <param name="userDeviceRepository">The user device repository.</param>
        /// <param name="userBadgeRepository">The user badge repository.</param>
        /// <param name="appointmentNotifyRepository">The appointment notify repository.</param>
        public NotificationService(IRepository<UserDevice> userDeviceRepository,
                                   IRepository<UserBadge> userBadgeRepository,
                                   IRepository<AppointmentNotify> appointmentNotifyRepository,
                                   IRepository<ProposedAppointmentNotify> proposedAppointmentNotifyRepository)
        {
            _userDeviceRepository = userDeviceRepository;
            _userBadgeRepository = userBadgeRepository;
            _appointmentNotifyRepository = appointmentNotifyRepository;
            this._proposedAppointmentNotifyRepository = proposedAppointmentNotifyRepository;
        }

        #endregion


        #region Private Fields

        private readonly IRepository<UserBadge> _userBadgeRepository;
        private readonly IRepository<UserDevice> _userDeviceRepository;
        private readonly IRepository<AppointmentNotify> _appointmentNotifyRepository;
        private readonly IRepository<ProposedAppointmentNotify> _proposedAppointmentNotifyRepository;
        #endregion


        #region Public Methods

        /// <summary>
        /// Get UserDevice By CustId
        /// </summary>
        /// <param name="custId"></param>
        /// <returns></returns>
        public IQueryable<UserDevice> GetAllUserDevice()
        {
            return _userDeviceRepository.Table;
        }
        /// <summary>
        /// Sends the notification.
        /// </summary>
        /// <param name="custId">The cust id.</param>
        /// <param name="message">The message.</param>
        /// <param name="data">The data.</param>
        /// <param name="sound">The sound.</param>
        public void SendNotification(int custId, string message, Types.NotificationType notificationType, Dictionary<string, object> data)
        {

            var userBadge = _userBadgeRepository.Table.FirstOrDefault(u => u.CustID == custId) ?? new UserBadge();
            var custDevices = _userDeviceRepository.Table.Where(d => d.CustId == custId);

            // For service version 1: send notification about appointment status change only
            int notificationsNumbersV1 = _appointmentNotifyRepository.Table.Count(a => a.CustId == custId
                && a.IsRead == false
                && (a.NotifyType == (int)Types.NotificationType.AppointmentCancelled
                    || a.NotifyType == (int)Types.NotificationType.AppointmentConfirmed
                    || a.NotifyType == (int)Types.NotificationType.AppointmentDeleted
                    || a.NotifyType == (int)Types.NotificationType.AppointmentModified));

            // For service version 2: send notification for appointment status change + appointment has note
            int notificationsNumbersV2 = _appointmentNotifyRepository.Table.Count(a => a.CustId == custId
                && a.IsRead == false
                && (a.NotifyType == (int)Types.NotificationType.AppointmentNote));



            var iosDevices = custDevices.Where(d => d.DeviceType == (int)Types.DeviceType.iOS).ToList();
            var androidDevices = custDevices.Where(d => d.DeviceType == (int)Types.DeviceType.Android).ToList();
            if (iosDevices.Any() || androidDevices.Any())
            {
                Task task = new Task(() =>
                    {


                        int index = 0;

                        var push = new PushBroker();
                        push.OnNotificationSent += NotificationSent;
                        push.OnNotificationFailed += NotificationFailed;

                        while (index > iosDevices.Count() || index < androidDevices.Count())
                        {
                            var iosDevice = iosDevices.Skip(index).FirstOrDefault();
                            // Send to IOS
                            if (iosDevice != null)
                            {
                                try
                                {
                                    AppleNotification notification = new AppleNotification();
                                    notification.ForDeviceToken(iosDevice.DeviceID)
                                                .WithAlert(message)
                                                .WithSound("aps.wav");

                                    bool isSend = true;
                                    if (iosDevice.ServiceVersion == (int)Types.ServiceVersion.AppointmentStatus)
                                    {
                                        notification.WithBadge(notificationsNumbersV1);
                                        if (notificationType == Types.NotificationType.AppointmentNote)
                                            isSend = false;
                                    }
                                    else if (iosDevice.ServiceVersion == (int)Types.ServiceVersion.AppointmentNote)
                                    {
                                        notification.WithBadge(notificationsNumbersV1 + notificationsNumbersV2);
                                    }

                                    if (isSend)
                                    {
                                        push = RegisterIosService(push, iosDevice.AppId);

                                        foreach (KeyValuePair<string, object> valuePair in data)
                                        {
                                            notification.WithCustomItem(valuePair.Key, valuePair.Value);
                                        }

                                        push.QueueNotification(notification);
                                    }
                                }
                                catch (Exception exception)
                                {
                                    LogHelper.Error("Can't send ios notification for cust: " + custId, exception);
                                }

                            }

                            var androidDevice = androidDevices.Skip(index).FirstOrDefault();
                            // Send to Android
                            if (androidDevice != null)
                            {

                                try
                                {
                                    GcmNotification notification = new GcmNotification();
                                    notification.ForDeviceRegistrationId(androidDevice.DeviceID);

                                    var androidData = new Dictionary<string, object>();
                                    foreach (KeyValuePair<string, object> valuePair in data)
                                    {
                                        androidData.Add(valuePair.Key, valuePair.Value);
                                    }

                                    bool isSend = true;
                                    if (androidDevice.ServiceVersion == (int)Types.ServiceVersion.AppointmentStatus)
                                    {
                                        androidData.Add("aps", new
                                        {
                                            alert = message,
                                            badge = notificationsNumbersV1
                                        });

                                        if (notificationType == Types.NotificationType.AppointmentNote)
                                            isSend = false;
                                    }
                                    else if (androidDevice.ServiceVersion == (int)Types.ServiceVersion.AppointmentNote)
                                    {
                                        if (notificationType == Types.NotificationType.AppointmentNote)
                                        {
                                            androidData.Add("aps", new
                                            {
                                                alert = message,
                                                badge = notificationsNumbersV2
                                            });
                                        }
                                        else
                                        {
                                            androidData.Add("aps", new
                                            {
                                                alert = message,
                                                badge = notificationsNumbersV1
                                            });
                                        }
                                    }

                                    if (isSend)
                                    {
                                        push = RegisterAndroidService(push, androidDevice.AppId);

                                        notification.WithJson(JObject.FromObject(androidData).ToString());

                                        push.QueueNotification(notification);
                                    }
                                }
                                catch (Exception exception)
                                {
                                    LogHelper.Error("Can't send android notification for cust: " + custId, exception);
                                }
                            }

                            push.StopAllServices();

                            index++;

                        }

                    });
                task.Start();
            }
        }

        public void SendNotification(int custId, string message, int notificationsNumbers, Types.NotificationType notificationType, Dictionary<string, object> data)
        {
            var custDevices = _userDeviceRepository.Table.Where(d => d.CustId == custId);


            var push = new PushBroker();
            push.OnNotificationSent += NotificationSent;
            push.OnNotificationFailed += NotificationFailed;

            foreach (var devices in custDevices)
            {
                if (devices.DeviceType == (int)Types.DeviceType.iOS)
                {
                    try
                    {
                        AppleNotification notification = new AppleNotification();
                        notification.ForDeviceToken(devices.DeviceID)
                                    .WithAlert(message)
                                    .WithSound("aps.wav");

                        notification.WithBadge(1);

                        push = RegisterIosService(push, devices.AppId);

                        foreach (KeyValuePair<string, object> valuePair in data)
                        {
                            notification.WithCustomItem(valuePair.Key, valuePair.Value);
                        }

                        push.QueueNotification(notification);

                    }
                    catch (Exception exception)
                    {
                        LogHelper.Error("Can't send ios notification for cust: " + custId, exception);
                    }

                }

                if (devices.DeviceType == (int)Types.DeviceType.Android)
                {
                    try
                    {
                        GcmNotification notification = new GcmNotification();
                        notification.ForDeviceRegistrationId(devices.DeviceID);

                        var androidData = new Dictionary<string, object>();
                        foreach (KeyValuePair<string, object> valuePair in data)
                        {
                            androidData.Add(valuePair.Key, valuePair.Value);
                        }

                        androidData.Add("aps", new
                        {
                            alert = message,
                            badge = notificationsNumbers
                        });

                        push = RegisterAndroidService(push, devices.AppId);
                        notification.WithJson(JObject.FromObject(androidData).ToString());
                        push.QueueNotification(notification);

                    }
                    catch (Exception exception)
                    {
                        LogHelper.Error("Can't send ios notification for cust: " + custId, exception);
                    }

                }
            }
            push.StopAllServices();
        }



        /// <summary>
        /// Adds the user badge unread.
        /// </summary>
        /// <param name="custId">The cust id.</param>
        public void AddUserBadgeUnread(int custId)
        {
            var userBadge = _userBadgeRepository.Table.FirstOrDefault(u => u.CustID == custId);
            if (userBadge == null)
            {
                userBadge = new UserBadge { CustID = custId, TotalSent = 1, BadgeUnread = 1 };
                _userBadgeRepository.Insert(userBadge);
            }
            else
            {
                userBadge.TotalSent += 1;
                userBadge.BadgeUnread += 1;
                _userBadgeRepository.Update(userBadge);
            }
        }


        /// <summary>
        /// Updates the user badge unread.
        /// </summary>
        /// <param name="notifyId">The notify id.</param>
        /// <param name="notifyType">Type of the notify.</param>
        /// <returns></returns>
        public bool ReadNotification(int notifyId, int notifyType)
        {
            if (notifyType >= (int)Types.NotificationType.AppointmentConfirmed
                && notifyType <= (int)Types.NotificationType.AppointmentDeleted)
            {
                var appNotify = _appointmentNotifyRepository.Table.FirstOrDefault(n => n.NotifyId == notifyId);
                if (appNotify != null && appNotify.IsRead == false)
                {
                    appNotify.IsRead = true;
                    var userBadge = _userBadgeRepository.Table.FirstOrDefault(u => u.CustID == appNotify.CustId);
                    if (userBadge != null)
                        userBadge.BadgeUnread -= 1;
                    _appointmentNotifyRepository.Update(appNotify);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Sends the appoinment change notify.
        /// </summary>
        /// <param name="appointmentId">The appointment id.</param>
        public void SendAppointmentChangeNotify(int appointmentId)
        {
            //var task = new Task(() =>
            //    {
            Appointment appointment = DAL.GetAppointmentById(appointmentId);
            if (appointment != null)
            {
                string action =
                    ((Types.AppointmentStatus)appointment.AppointmentStatusID).ToString().ToLower();
                int notifyType = (int)Types.NotificationType.AppointmentConfirmed;
                switch (appointment.AppointmentStatusID)
                {
                    case (int)Types.AppointmentStatus.Confirmed:
                        action = "confirmed";
                        notifyType = (int)Types.NotificationType.AppointmentConfirmed;
                        break;
                    case (int)Types.AppointmentStatus.Cancelled:
                        action = "cancelled";
                        notifyType = (int)Types.NotificationType.AppointmentCancelled;
                        break;
                    case (int)Types.AppointmentStatus.CompanyModified:
                        action = "modified";
                        notifyType = (int)Types.NotificationType.AppointmentModified;
                        break;
                    case (int)Types.AppointmentStatus.Delete:
                        action = "deleted";
                        notifyType = (int)Types.NotificationType.AppointmentDeleted;
                        break;
                }


                string message = string.Format("{0} {1} your appointment", appointment.ProfileCompany != null ? appointment.ProfileCompany.Name : string.Empty, action);

                AppointmentNotify appointmentNotify = new AppointmentNotify();
                appointmentNotify.AppointmentId = appointment.AppointmentID;
                appointmentNotify.DateSent = DateTime.UtcNow;
                appointmentNotify.Message = message;
                appointmentNotify.NotifyType = notifyType;
                appointmentNotify.IsRead = false;
                appointmentNotify.CustId = appointment.CustID;
                _appointmentNotifyRepository.Insert(appointmentNotify);

                var data = new Dictionary<string, object>();
                data.Add("custId", appointmentNotify.CustId);
                data.Add("appId", appointmentNotify.AppointmentId);
                data.Add("notifyId", appointmentNotify.NotifyId);
                data.Add("type", notifyType);
                SendNotification(appointment.CustID, message, (Types.NotificationType)appointment.AppointmentStatusID, data);
            }
            //    });
            //task.Start();
        }

        /// <summary>
        /// Send notification about new appointment note.
        /// </summary>
        /// <param name="note">The note.</param>
        public void SendAppointmentNote(AppointmentLog note, int custId)
        {
            AppointmentNotify appointmentNotify = new AppointmentNotify()
                {
                    CustId = custId,
                    NotifyType = (int)Types.NotificationType.AppointmentNote,
                    AppointmentId = note.AppointmentID,
                    DateSent = DateTime.UtcNow,
                    Message = "your appointment got a new note",
                    IsRead = false
                };
            _appointmentNotifyRepository.Insert(appointmentNotify);
            var data = new Dictionary<string, object>();
            data.Add("custId", appointmentNotify.CustId);
            data.Add("appId", appointmentNotify.AppointmentId);
            data.Add("notifyId", appointmentNotify.NotifyId);
            data.Add("type", appointmentNotify.NotifyType);
            SendNotification(appointmentNotify.CustId, appointmentNotify.Message, Types.NotificationType.AppointmentNote, data);
        }

        /// <summary>
        ///  Send notification Proposed Appointment 
        /// </summary>
        /// <param name="appointment"></param>
        public void SendProposedAppointment(ProposedAppointment appointment)
        {
            ProposedAppointmentNotify appointmentNotify = new ProposedAppointmentNotify()
            {
                CustId = appointment.CustID,
                NotifyType = (int)Types.NotificationType.ProposedAppointment,
                AppointmentId = appointment.AppointmentID,
                DateSent = DateTime.UtcNow,
                Message = "you have a new proposed appointment",
                IsRead = false
            };
            _proposedAppointmentNotifyRepository.Insert(appointmentNotify);
            var data = new Dictionary<string, object>();
            data.Add("custId", appointmentNotify.CustId);
            data.Add("appId", appointmentNotify.AppointmentId);
            data.Add("notifyId", appointmentNotify.NotifyId);
            data.Add("type", appointmentNotify.NotifyType);

            int notificationsNumbers = _proposedAppointmentNotifyRepository.Table.Count(a => a.CustId == appointment.CustID && a.IsRead == false && (a.NotifyType == (int)Types.NotificationType.ProposedAppointment));

            SendNotification(appointmentNotify.CustId, appointmentNotify.Message, notificationsNumbers, Types.NotificationType.ProposedAppointment, data);
        }


        public void ProposedPushMessage(ProposedAppointment appointment, string message)
        {
            ProposedAppointmentNotify appointmentNotify = new ProposedAppointmentNotify()
            {
                CustId = appointment.CustID,
                NotifyType = (int)Types.NotificationType.ProposedAppointment,
                AppointmentId = appointment.AppointmentID,
                DateSent = DateTime.UtcNow,
                Message = message,
                IsRead = false
            };
            var data = new Dictionary<string, object>();
            data.Add("custId", appointmentNotify.CustId);
            data.Add("appId", appointmentNotify.AppointmentId);
            data.Add("notifyId", appointmentNotify.NotifyId);
            data.Add("type", appointmentNotify.NotifyType);

            int notificationsNumbers = _proposedAppointmentNotifyRepository.Table.Count(a => a.CustId == appointment.CustID && a.IsRead == false && (a.NotifyType == (int)Types.NotificationType.ProposedAppointment));

            SendNotification(appointmentNotify.CustId, appointmentNotify.Message, notificationsNumbers, Types.NotificationType.ProposedAppointment, data);
        }


        public void PushGeneralMessage(int custId, string message, int notificationsNumbers, Types.NotificationType notificationType)
        {
            var data = new Dictionary<string, object>();
            data.Add("custId", custId);
            data.Add("type", (int)notificationType);
            SendNotification(custId, message, notificationsNumbers, notificationType, data);
        }
        /// <summary>
        /// Updates the unread notification.
        /// </summary>
        /// <param name="custId">The cust id.</param>
        /// <param name="unreadRemain">The unread remain.</param>
        /// <returns></returns>
        public bool UpdateUnreadNotification(int custId, int unreadRemain)
        {
            var userBadge = _userBadgeRepository.Table.FirstOrDefault(u => u.CustID == custId);

            if (userBadge != null)
            {
                userBadge.BadgeUnread = unreadRemain;
                _userBadgeRepository.Update(userBadge);
                return true;
            }
            return false;
        }


        /// <summary>
        /// Gets the appoinment notifications.
        /// </summary>
        /// <param name="custId">The cust id.</param>
        /// <returns></returns>
        public IQueryable<AppointmentNotify> GetAppoinmentNotifications(int custId)
        {
            return _appointmentNotifyRepository.Table.Where(n => n.CustId == custId);
        }

        public IQueryable<ProposedAppointmentNotify> GetPorposedAppoinmentNotifications(int custId)
        {
            return _proposedAppointmentNotifyRepository.Table.Where(n => n.CustId == custId);
        }

        public List<AppointmentNotify> GetHybridAppoinmentNotifications(int custId, DateTime currentDate)
        {
            var query = _appointmentNotifyRepository.Table.Where(n => n.CustId == custId && n.IsRead == false).ToList();
            var query1 = _proposedAppointmentNotifyRepository.Table.Where(n => n.CustId == custId
                && n.ProposedAppointment.Start > currentDate
                && n.ProposedAppointment.AppointmentStatusID == (int)Types.ProposedAppointmentStatus.Default).ToList()
                .Select(m => new AppointmentNotify
                {
                    AppointmentId = m.AppointmentId,
                    CustId = m.CustId,
                    DateSent = m.DateSent,
                    Message = m.Message,
                    NotifyId = m.NotifyId,
                    NotifyType = m.NotifyType,
                    IsRead = m.IsRead
                }).ToList();
            var query2 = query.Union(query1);
            return query2.ToList();
        }

        /// <summary>
        /// Tests the IOS.
        /// </summary>
        /// <param name="deviceId">The device id.</param>
        public bool TestIOSNotification(string deviceId)
        {
            try
            {
                var push = new PushBroker();
                push.OnNotificationSent += NotificationSent;
                push.OnNotificationFailed += NotificationFailed;


                var appleCert =
                    File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                                   ConfigurationManager.AppSettings["AppleAPNSFile"]));

                push.RegisterAppleService(new ApplePushChannelSettings(appleCert,
                                                                       ConfigurationManager.AppSettings[
                                                                           "AppleAPNSPass"]));
                AppleNotification notification = new AppleNotification();
                notification.ForDeviceToken(deviceId)
                            .WithAlert("Test iOS push notification")
                            .WithBadge(3);


                push.QueueNotification(notification);

                push.StopAllServices();
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error("Send IOS notification fail", ex);
            }
            return false;
        }


        /// <summary>
        /// Tests the IOS notification.
        /// </summary>
        /// <param name="deviceId">The device id.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public bool TestIOSNotification(string deviceId, out Exception exception)
        {
            exception = null;
            try
            {
                var push = new PushBroker();
                push.OnNotificationSent += NotificationSent;
                push.OnNotificationFailed += NotificationFailed;

                byte[] appleCert = null;
                bool getCert = false;
                try
                {
                    appleCert =
                        File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                                       ConfigurationManager.AppSettings["AppleAPNSFile"]));
                    getCert = true;
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                if (!getCert)
                {
                    try
                    {
                        appleCert =
                            File.ReadAllBytes(
                                HttpContext.Current.Server.MapPath("~/" +
                                                                   ConfigurationManager.AppSettings["AppleAPNSFile"]));
                    }
                    catch
                    {
                        return false;
                    }
                }

                push.RegisterAppleService(new ApplePushChannelSettings(appleCert,
                                                                       ConfigurationManager.AppSettings[
                                                                           "AppleAPNSPass"]));
                AppleNotification notification = new AppleNotification();
                notification.ForDeviceToken(deviceId)
                            .WithAlert("Test iOS push notification")
                            .WithBadge(3);


                push.QueueNotification(notification);

                push.StopAllServices();
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error("Send IOS notification fail", ex);
                exception = ex;
            }
            return false;
        }


        /// <summary>
        /// Tests the android notification.
        /// </summary>
        /// <param name="deviceId">The device id.</param>
        /// <returns></returns>
        public bool TestAndroidNotification(string deviceId)
        {
            try
            {
                var push = new PushBroker();
                push.OnNotificationSent += NotificationSent;
                push.OnNotificationFailed += NotificationFailed;


                push.RegisterGcmService(new GcmPushChannelSettings(ConfigurationManager.AppSettings["AndroidGCM"]));
                GcmNotification notification = new GcmNotification();
                notification.ForDeviceRegistrationId(deviceId);
                var data = new Dictionary<string, object>();
                data.Add("aps", new
                    {
                        alert = "test Android notification",
                        badge = 3
                    });
                notification.WithJson(JObject.FromObject(data).ToString());

                push.QueueNotification(notification);

                push.StopAllServices();
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error("Send Android notification fail", ex);
            }
            return false;
        }


        /// <summary>
        /// Clears the appointment notifications.
        /// </summary>
        /// <param name="appointmentId">The appointment id.</param>
        /// <param name="notifyTypes">The notify types.</param>
        public void ClearAppointmentNotifications(int appointmentId, List<int> notifyTypes)
        {
            try
            {
                var notifications = _appointmentNotifyRepository.Table.Where(n => n.AppointmentId == appointmentId
                                                                                  && notifyTypes.Contains(n.NotifyType));
                foreach (var appointmentNotify in notifications)
                {
                    appointmentNotify.IsRead = true;
                }
                _appointmentNotifyRepository.Update(new AppointmentNotify());
            }
            catch (Exception)
            {

            }
        }


        public void ClearProposedAppointmentNotifications(int appointmentId)
        {
            try
            {
                var notifications = _proposedAppointmentNotifyRepository.Table.Where(n => n.AppointmentId == appointmentId);
                foreach (var appointmentNotify in notifications)
                {
                    appointmentNotify.IsRead = true;
                }
                _proposedAppointmentNotifyRepository.Update(new ProposedAppointmentNotify());
            }
            catch (Exception)
            {

            }
        }

        #endregion


        #region Private Methods

        /// <summary>
        /// Notifications the sent.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="notification">The notification.</param>
        private void NotificationSent(object sender, INotification notification)
        {
            string deviceId = string.Empty;
            Types.DeviceType type = Types.DeviceType.iOS;
            if (notification.GetType() == typeof(AppleNotification))
            {
                deviceId = ((AppleNotification)notification).DeviceToken;
                type = Types.DeviceType.iOS;
            }
            else if (notification.GetType() == typeof(GcmNotification))
            {
                deviceId = ((GcmNotification)notification).RegistrationIds.FirstOrDefault();
                type = Types.DeviceType.Android;
            }

            UserDevice userDevice = _userDeviceRepository.Table.Where(d => d.DeviceType == (int)type && d.DeviceID == deviceId).OrderByDescending(d => d.DateUsage).FirstOrDefault();
            if (userDevice != null)
            {
                AddUserBadgeUnread(userDevice.CustId);
                LogHelper.Info(string.Format("Send notification SUCCESS to cust: {0} - message {1}", userDevice.CustId.ToString(), notification));
            }

        }

        /// <summary>
        /// Notifications the failed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="notification">The notification.</param>
        /// <param name="notificationFailureException">The notification failure exception.</param>
        private void NotificationFailed(object sender, INotification notification,
                                        Exception notificationFailureException)
        {
            string deviceId = string.Empty;
            Types.DeviceType type = Types.DeviceType.iOS;
            if (notification.GetType() == typeof(AppleNotification))
            {
                deviceId = ((AppleNotification)notification).DeviceToken;
                type = Types.DeviceType.iOS;
            }
            else if (notification.GetType() == typeof(GcmNotification))
            {
                deviceId = ((GcmNotification)notification).RegistrationIds.FirstOrDefault();
                type = Types.DeviceType.Android;
            }

            UserDevice userDevice =
                _userDeviceRepository.Table.Where(d => d.DeviceType == (int)type && d.DeviceID == deviceId).OrderByDescending(d => d.DateUsage).FirstOrDefault();
            if (userDevice != null)
            {
                AddUserBadgeUnread(userDevice.CustId);
                LogHelper.Error(
                    string.Format("Send notification FAILURE to cust: {0} - message {1}",
                                  userDevice.CustId.ToString(CultureInfo.InvariantCulture), notification),
                    notificationFailureException);
            }
        }


        /// <summary>
        /// Registers the ios service.
        /// </summary>
        /// <param name="pushBroker">The push broker.</param>
        /// <param name="appId">The app id.</param>
        /// <returns></returns>
        private PushBroker RegisterIosService(PushBroker pushBroker, int appId)
        {
            byte[] appleCert = null;
            if (pushBroker == null)
            {
                pushBroker = new PushBroker();
                pushBroker.OnNotificationSent += NotificationSent;
                pushBroker.OnNotificationFailed += NotificationFailed;
            }

            string appleCertFile = ConfigurationManager.AppSettings["AppleAPNSFile"];
            string appleCertPass = ConfigurationManager.AppSettings["AppleAPNSPass"];

            if (appId == (int)Types.ApplicationType.Concierge)
            {
                appleCertFile = ConfigurationManager.AppSettings["AppleAPNSFileConcierge"];
                appleCertPass = ConfigurationManager.AppSettings["AppleAPNSPassConcierge"];
            }
            // Get certificate file
            try
            {
                appleCert = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, appleCertFile));
            }
            catch
            {
                string filePath = "~/" + ConfigurationManager.AppSettings["AppleAPNSFile"];
                appleCert = File.ReadAllBytes(HttpContext.Current.Server.MapPath(filePath.Replace("//", "/")));
            }

            // Register service
            pushBroker.RegisterAppleService(new ApplePushChannelSettings(appleCert, appleCertPass));
            return pushBroker;
        }

        /// <summary>
        /// Registers the android service.
        /// </summary>
        /// <param name="pushBroker">The push broker.</param>
        /// <param name="appId">The app id.</param>
        /// <returns></returns>
        private PushBroker RegisterAndroidService(PushBroker pushBroker, int appId)
        {
            byte[] appleCert = null;
            if (pushBroker == null)
            {
                pushBroker = new PushBroker();
                pushBroker.OnNotificationSent += NotificationSent;
                pushBroker.OnNotificationFailed += NotificationFailed;
            }

            string androidCerf = ConfigurationManager.AppSettings["AndroidGCM"];
            if (appId == (int)Types.ApplicationType.Concierge)
                androidCerf = ConfigurationManager.AppSettings["AndroidGCMConcierge"];

            pushBroker.RegisterGcmService(new GcmPushChannelSettings(androidCerf));
            return pushBroker;
        }
        #endregion

    }
}
