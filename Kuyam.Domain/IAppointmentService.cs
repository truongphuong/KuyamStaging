﻿using Kuyam.Database;
using Kuyam.Domain.AppointmentModel;
using Kuyam.Domain.ClassModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuyam.Domain.Models;

namespace Kuyam.Domain
{
    public interface IAppointmentService
    {
        void AddAppointment(Appointment appointment);   
        void InsertAppointment(Appointment appointment);
        void UpdateAppointment(Appointment appointment);
        int UpdateAppointment(int appointmentID, int rating);
        void DeleteAppointment(Appointment appointment);
        void InsertAppointmentTemp(AppointmentTemp appointmentTemp);
        void UpdateAppointmentTemp(AppointmentTemp appointmentTemp);
        void DeleteAppointmentTemp(AppointmentTemp appointmentTemp);
        Appointment GetAppointmentByID(int appoimentID);
        List<Service> GetListService();
        Service GetServiceById(int Id);
        Calendar GetCalendarIdById(int calendarId);
        List<CompanyEmployee> GetCompanyEmployeeByCompanyID(int companyId);
        List<AppointmentLog> GetAppointmentNoteByID(int aptId);
        List<ServiceCompany> GetListEmployeeServiceByEmployeeID(int companyEmployeeID);
        List<Appointment> GetListAppointmentByCustID(int custID);
        List<Appointment> GetleftAppointmentByCustID(int custID);
        List<AppointmentParticipant> GetListAppointmentParticipantCustID(int custID, DateTime? createdDate, bool? isloadMore, int calendarId = 0, int status = 0, int seviceid = 0);
        List<AppointmentParticipant> GetListAppointmentParticipantCustID2(int custID, DateTime? startDate, bool? isloadMore, int calendarId = 0, int status = 0, int seviceid = 0);
        void RequestModification(Appointment appointment);
        void AddNote(AppointmentLog aptlg);
        void AddRating(Rating rating);
        void ViewedNote(List<AppointmentLog> aptlgs);
        Appointment ChangeStatus(int appointmentID, int AppointmentStatusID, string reason = "");       
        List<Appointment> GetAppoinmentsByEmployeeId(int employeeId, DateTime startDate, DateTime endDate);
        List<Appointment> GetAppoinmentsByEmployeeId(int employeeId, int serviceId, DateTime startDate, DateTime endDate);
        List<AppointmentTemp> GetAppoinmentTempsByEmployeeId(int employeeId, int calendarId, DateTime startDate, DateTime endDate);
        List<AppointmentBusyTime> GetAppoinmentBusyTimByEmployeeId(int profileId);
        AppointmentTemp GetAppoinmentTempsById(int id);
        List<Appointment> GetAppoinmentsByEmployeeIdForIP(int employeeId, DateTime startDate, DateTime endDate);
        List<Rating> GetRatingsByProfileId(int profileId);
        List<RatingModel> GetRatingByCompanyProfile(int companyProfileId);
        List<Rating> GetRatingListByProfileID(int profileID, int pageIndex, int pageSize, out int totalRecord);
        List<Appointment> GetAppoinmentsByCustId(int custID, DateTime startDate, DateTime endDate);
        List<PaymentMethod> GetPaymentMethodByProfileId(int profileId);
        List<Appointment> GetAppointmentByCalendarId(int calendaId, DateTime startDate, DateTime endDate);
        List<Calendar> GetAppointmentByCalendarId(int calendaId, DateTime startDate);
        List<Appointment> GetActiveAppointmentsByCalendarId(int calendaId, DateTime startDate, DateTime endDate);
        List<RequestAppointment> GetRequestAppointmentsByCalendarId(int calendaId, DateTime startDate, DateTime endDate);
        List<RequestAppointment> GetRequestAppointmentsByCalendarId(int calendaId, List<DateTime> statDate);      
        AppointmentParticipant GetAppointmentParticipantByAppointmentId(int appointmentId);
        List<EmployeeHour> GetListEmployeeHour(int employeeId);
        List<Service> GetServicesByCustomerId(int custId);
        List<Appointment> GetAppoinmentsByProfileId(int profileId, DateTime startTime, DateTime endTime);
        List<Appointment> GetAppointmentHistoryNotReview(int custID, DateTime? day, int calendarId = 0, DateTime currentDate = default(DateTime));
        List<Appointment> GetAppointmentHistoryReview(int custID, DateTime day, int calendarId = 0, DateTime currentDate = default(DateTime));
        List<Rating> GetRatingByServiceCompanyId(int id);
        List<Appointment> GetAppointmentsHaveNote(Cust cust, ProfileCompany profile, DateTime fromDate);
        NonKuyamAppointment GetLastNonKuyamAppointment(int custId, int profileId = 0);
        NonKuyamAppointment GetLastNonKuyamAppointmentById(int apptId);
        IQueryable<NonKuyamAppointment> GetNonKuyamAppointments(int custId);
        bool AddNonKuyamAppointment(NonKuyamAppointment nonKuyamAppointment);
        bool UpdateNonKuyamAppointment(NonKuyamAppointment nonKuyamAppointment);
        bool UpdateProposedKuyamAppointment(ProposedAppointment proposedAppointment);
        bool UpdateRequestAppointment(RequestAppointment requestAppointment);
        Appointment GetAppointmentReview(int custId);
        bool AddRequestAppointment(List<RequestAppointment> items);
        List<RequestAppointment> GetRequestAppointments(int companyId, DateTime? startDate, DateTime? endDate, int pageIndex, int pageSize, out int totalRecord);
        RequestAppointment GetRequestAppointmentsById(int Id);
        ProposedAppointment InsertProposedAppointment(ProposedAppointment proposed);
        ProposedAppointment GetProposedAppointmentById(int apptId);
        List<ProposedAppointment> GetProposedAppointment(int companyId, int pageIndex, int pageSize, out int totalRecord);
        List<ProposedAppointment> GetProposedAppointmentsByCustId(int custId, int? calendarId, DateTime currentDate);
        List<ProposedAppointment> GetProposedAppointmentsByCustId(int custId, int? calendarId, DateTime currentDate, int pageIndex, int pageSize, out int totalRecord);
        List<ProposedAppointment> GetProposedAppointmentsByHotelId(int hotelId, DateTime currentDate, int pageIndex, int pageSize, string key, out int totalRecord);       
        List<CompanyTimeSlot> GetCompanyTimeSlotsAvailable(int profileId, DateTime startTime, DateTime endTime, int take = 0, int timeSlot = 30);
        SchedulerAvailability GetDataCheckOutOfClass(int classSchedulerId);
        SchedulerAvailability GetDataCheckOutForServiceIdAndEmployeeId(int serviceId, int employeeId);
        void UpdateInstructorClassScheduler(InstructorClassScheduler classScheduler);
        InstructorClassScheduler GetInstructorClassSchedulerById(int classSchedulerById);
    }
}
