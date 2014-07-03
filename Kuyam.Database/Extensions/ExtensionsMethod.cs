using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuyam.Database.Extensions
{
    public static class ExtensionsMethod
    {
        public static Appointment ToAppointment(this AppointmentTemp appointmentTemp)
        {
            return new Appointment
            {
                AppointmentID = 0,
                ProfileId = appointmentTemp.ProfileId,
                Title = appointmentTemp.Title,
                Notes = appointmentTemp.Notes,
                Desc = appointmentTemp.Desc,
                Rating = appointmentTemp.Rating,
                Url = appointmentTemp.Url,
                StatusChangeDate = appointmentTemp.StatusChangeDate,
                AllDay = appointmentTemp.AllDay,
                AppointmentStatusID = (int)Types.AppointmentStatus.TemporaryPending,
                BookingType = (int)Types.BookingType.ClassBooking,
                ContactPerson = appointmentTemp.ContactPerson,
                ContactType = appointmentTemp.ContactType,
                CustID = appointmentTemp.CustID,
                CalendarId = appointmentTemp.CalendarId,
                ServiceCompanyID = appointmentTemp.ServiceCompanyID,                
                EmployeeID = appointmentTemp.EmployeeID,
                StaffID = appointmentTemp.StaffID,
                HotelID = appointmentTemp.HotelID,
                ClassSchedulerID = appointmentTemp.ClassSchedulerID,
                Start = appointmentTemp.Start,
                End = appointmentTemp.End,
                ServiceName = appointmentTemp.ServiceName,
                EmployeeName = appointmentTemp.EmployeeName,
                Price = appointmentTemp.Price,
                Duration = appointmentTemp.Duration,
                AttendeesNumber = appointmentTemp.AttendeesNumber,
                PersonCount = appointmentTemp.PersonCount,
                SenderEmail = appointmentTemp.SenderEmail,
                PreapprovalKey = appointmentTemp.PreapprovalKey,
                Created = appointmentTemp.Created,
                Modified = appointmentTemp.Modified

            };
        }
        public static Appointment ToAppointment(this NonKuyamAppointment nonKuyamAppointment)
        {
            return new Appointment
            {
                AppointmentID = nonKuyamAppointment.AppointmentID,
                ProfileId = nonKuyamAppointment.ProfileId,
                Notes = nonKuyamAppointment.Notes,
                StatusChangeDate = nonKuyamAppointment.StatusChangeDate,
                AppointmentStatusID = (int)Types.AppointmentStatus.TemporaryPending,
                CustID = nonKuyamAppointment.CustID,
                CalendarId = nonKuyamAppointment.CalendarId ?? 0,
                StaffID = nonKuyamAppointment.StaffID,
                HotelID = nonKuyamAppointment.HotelID,
                Start = nonKuyamAppointment.Start,
                End = nonKuyamAppointment.End,
                ServiceName = nonKuyamAppointment.ServiceName,
                EmployeeName = nonKuyamAppointment.EmployeeName,
                Price = nonKuyamAppointment.Price,
                Duration = nonKuyamAppointment.Duration,
                AttendeesNumber = nonKuyamAppointment.AttendeesNumber,
                Created = nonKuyamAppointment.Created,
                Modified = nonKuyamAppointment.Modified

            };
        }

        public static Appointment ToAppointment(this ProposedAppointment proposedAppointment)
        {
            return new Appointment
            {
                AppointmentID = proposedAppointment.AppointmentID,
                ProfileId = proposedAppointment.ProfileId,
                Notes = proposedAppointment.Notes,
                StatusChangeDate = proposedAppointment.StatusChangeDate,
                AppointmentStatusID = (int)Types.AppointmentStatus.TemporaryPending,
                CustID = proposedAppointment.CustID,
                CalendarId = proposedAppointment.CalendarId ?? 0,
                StaffID = proposedAppointment.StaffID,
                HotelID = proposedAppointment.HotelID,
                Start = proposedAppointment.Start,
                End = proposedAppointment.End,
                ServiceName = proposedAppointment.ServiceName,
                EmployeeName = proposedAppointment.EmployeeName,
                Price = proposedAppointment.Price,
                Duration = proposedAppointment.Duration,
                AttendeesNumber = proposedAppointment.AttendeesNumber,
                Created = proposedAppointment.Created,
                Modified = proposedAppointment.Modified

            };
        }
    }
}
