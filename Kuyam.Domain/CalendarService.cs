using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database;
using Kuyam.Repository;
using Kuyam.Repository.Interface;
using Kuyam.Utility;

namespace Kuyam.Domain
{
    public class CalendarService
    {
        #region Fields

        private readonly IRepository<Calendar> _calendarRepository;
        private readonly IRepository<CalendarShare> _calendarShareRepository;
        private readonly IRepository<AppointmentParticipant> _appointmentParticipantRepository;

        #endregion

        #region Ctor

        public CalendarService(IRepository<Calendar> calendarRepository,
                               IRepository<CalendarShare> calendarShareRepository,
                               IRepository<AppointmentParticipant> appointmentParticipantRepository)
        {

            this._calendarRepository = calendarRepository;
            this._calendarShareRepository = calendarShareRepository;
            this._appointmentParticipantRepository = appointmentParticipantRepository;

        }

        #endregion

        #region Functions

        /// <summary>
        /// Get all calendar
        /// </summary>
        /// <param name="custId">int: customer id</param>
        /// <returns>List: calendars</returns>
        public List<Calendar> GetActiveCalendarsbyCustId(int custId)
        {
            var lstCalendar = (from c in _calendarRepository.Table
                               join cs in _calendarShareRepository.Table on c.CalendarID equals cs.CalendarID
                               where cs.CustID == custId && c.CalendarDisplayTypeID != (int)Types.CalendarDisplayType.Hidden
                               select c).Distinct();
            return lstCalendar.ToList();
        }
        /// <summary>
        /// Insert new a calendar
        /// </summary>
        /// <param name="custID">int:customer id</param>
        /// <param name="name">string: calendar name</param>
        /// <returns>
        ///          true:success
        ///          false:unsuccess   
        /// </returns>
        public bool AddCalendar(int custID, string name, string backColor, int calendarType, string syncCalendarId)
        {
            bool result = false;
            try
            {
                var calendar = new Calendar()
                {

                    ProfileID = null,
                    Name = name,
                    BackColor = backColor,
                    ForeColor = "0",
                    IsDefault = false,
                    CalendarDisplayTypeID = calendarType,
                    Created = DateTime.UtcNow,
                    SyncCalendarId = syncCalendarId,
                    Modified = null
                };
                _calendarRepository.Insert(calendar);
                var calendarshare = new CalendarShare()
                {
                    Calendar = calendar,
                    CustID = custID,
                    ShareTypeID = (int)Types.CalendarType.Default,
                    Created = DateTime.UtcNow,
                    Modified = DateTime.UtcNow
                };
                _calendarShareRepository.Insert(calendarshare);
                LogHelper.Info(string.Format("Added calendar: CalendarID= {0}, CalendarShareID= {1}", calendar.CalendarID, calendarshare.CalendarShareID));
                result = true;
            }
            catch (Exception ex)
            {
                LogHelper.Error("Insert calendar fail:", ex);
                result = false;
            }
            return result;
        }
        /// <summary>
        /// Delete a calendar
        /// </summary>
        /// <param name="custID">int:customer id</param>
        /// <param name="calendarId">int: calendar id</param>
        /// <returns>
        ///          true:success
        ///          false:unsuccess   
        /// </returns>
        public bool DeleteCalendar(int custID, int calendarId)
        {
            bool result = false;
            try
            {
                var calendar = _calendarRepository.Table.FirstOrDefault(c => c.CalendarID == calendarId);
                if (calendar != null && !calendar.IsDefault && !CheckExistCalendarAppointmentParticipant(calendarId))
                {
                    var shareCalendar = _calendarShareRepository.Table.FirstOrDefault(s => s.CalendarID == calendarId && s.CustID == custID);
                    if (shareCalendar != null)
                    {
                        _calendarShareRepository.Delete(shareCalendar);
                        _calendarRepository.Delete(calendar);
                        LogHelper.Info(string.Format("Deleted calendars: CalendarID= {0}, CalendarShareID= {1}", calendar.CalendarID, shareCalendar.CalendarShareID));
                    }
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("Delete calendar fail:", ex);
                result = false;
            }
            return result;
        }
        /// <summary>
        /// Delete calendar list
        /// </summary>
        /// <param name="custID">Int: cust id</param>
        /// <param name="calendarType">Int: calendar type</param>
        public void DeleteCalendars(int custID, int calendarType)
        {
            List<CalendarShare> shareCalendars = _calendarShareRepository.Table.Where(s => s.CustID == custID).ToList();
            if (shareCalendars != null && shareCalendars.Count > 1)
            {
                foreach (CalendarShare shareCalendar in shareCalendars)
                {
                    Calendar calendar = _calendarRepository.Table.FirstOrDefault(c => c.CalendarID == shareCalendar.CalendarID);
                    if (calendar != null && !calendar.IsDefault && !CheckExistCalendarAppointmentParticipant(calendar.CalendarID)
                        && calendar.CalendarDisplayTypeID == calendarType)
                    {
                        if (shareCalendar != null)
                        {
                            try
                            {
                                _calendarShareRepository.Delete(shareCalendar);
                                _calendarRepository.Delete(calendar);
                                LogHelper.Info(string.Format("Deleted calendar: CalendarID= {0}, CalendarShareID= {1}", calendar.CalendarID, shareCalendar.CalendarShareID));
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error("Delete calendars fail:", ex);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Update a calendar by calendar id
        /// </summary>
        /// <param name="id">int: calendar id</param>
        /// <param name="name">string: calendar name</param>
        /// <param name="backColor">string: calendar olor</param>
        /// <returns>
        ///          true:success
        ///          false:unsuccess   
        /// </returns>
        public bool UpdateCalendar(int id, string name, string backColor)
        {
            bool result = false;
            var calendar = _calendarRepository.Table.FirstOrDefault(c => c.CalendarID == id);
            if (calendar != null)
            {
                try
                {
                    calendar.Name = name;
                    calendar.Modified = DateTime.UtcNow;
                    calendar.BackColor = backColor;
                    _calendarRepository.Update(calendar);
                    LogHelper.Info(string.Format("Updated calendar: CalendarID= {0}", calendar.CalendarID));
                    result = true;
                }
                catch (Exception ex)
                {
                    LogHelper.Error("Update calendar fail:", ex);
                    result = false;
                }
            }
            return result;
        }
        /// <summary>
        /// Get calendar by calenadr id
        /// </summary>
        /// <param name="id">int: calendar id</param>
        /// <returns>List calendar</returns>
        public Calendar GetCalendarByCalendarId(int id)
        {
            return _calendarRepository.Table.Where(c => c.CalendarID == id).FirstOrDefault();
        }
        /// <summary>
        /// Get calendar share by calendar id
        /// </summary>
        /// <param name="id">int: calendar id</param>
        /// <returns>Calendar list</returns>
        public List<CalendarShare> GetCalendarShareByCalendarId(int id)
        {
            return _calendarShareRepository.Table.Where(s => s.CalendarID == id).ToList();
        }
        /// <summary>
        /// Check calendar in AppointmentParticipant table
        /// </summary>
        /// <param name="calendarId">int: calendar id</param>
        /// <returns>
        ///          true:Exist in AppointmentParticipant table
        ///          false:Not exist in AppointmentParticipant table
        /// </returns>
        public bool CheckExistCalendarAppointmentParticipant(int calendarId)
        {
            bool result = false;
            List<AppointmentParticipant> appointmentParticipants = _appointmentParticipantRepository.Table.Where(a => a.CalendarID == calendarId).ToList();
            if (appointmentParticipants.Count > 0)
            {
                result = true;
            }
            return result;
        }

        #endregion
    }
}
