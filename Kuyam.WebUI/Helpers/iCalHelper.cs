using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kuyam.WebUI.Models;
using System.IO;
using Kuyam.Utility;

namespace Kuyam.WebUI.Helpers
{
    public class iCalHelper
    {
        /// <summary>
        /// Export to iCal
        /// </summary>
        /// <param name="eventList"></param>
        /// <param name="filePath"></param>
        public bool ExportCalendar(string filePath){

            Kuyam.Database.Appointment appointment = Kuyam.Database.ProfileCompany.GetAppointmentById(MySession.AppoimentID);
            try{
                // Create an iCalendar.
                DDay.iCal.IICalendar iCal = new DDay.iCal.iCalendar();

                // Create an event and attach it to the iCalendar.
                DDay.iCal.Event evt = iCal.Create<DDay.iCal.Event>();

                if (appointment.ServiceCompany != null && appointment.ServiceCompany.ProfileCompany != null){

                    DateTime start = appointment.Start;
                    DateTime end = appointment.End;

                    string description = string.Format("{0} {1} min, ${2}, {3} person",
                        appointment.ServiceCompany.Service.ServiceName,
                        appointment.ServiceCompany.Duration, appointment.ServiceCompany.Price,
                        appointment.ServiceCompany.AttendeesNumber);

                    evt.Summary = appointment.ServiceCompany.Service.ServiceName;
                    evt.Description = description;
                    evt.Start = new DDay.iCal.iCalDateTime(start);
                    evt.End = new DDay.iCal.iCalDateTime(end);
                    evt.Location = string.Empty;
                    evt.UID = MySession.AppoimentID.ToString();
                }
                //Save iCalendar with file path.
                DDay.iCal.Serialization.iCalendar.iCalendarSerializer serializer = new DDay.iCal.Serialization.iCalendar.iCalendarSerializer();
                serializer.Serialize(iCal, filePath);

                LogHelper.Info(string.Format("Export to iCal successful: iCal id={0}",evt.UID));

                return true;
            }
            catch (Exception ex){
                LogHelper.Error("Export to iCal is fail:", ex);
                return false;
            }
        }
    }
}