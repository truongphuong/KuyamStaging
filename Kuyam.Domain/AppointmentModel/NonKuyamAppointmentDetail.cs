using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kuyam.Database;

namespace Kuyam.Domain.AppointmentModel
{
    public class NonKuyamAppointmentDetail
    {
        public int AppointmentID { get; set; }
        public int? ServiceId { get; set; }
        public int AppointmentStatusID { get; set; }
        public String StatusChangeDate { get; set; }
        public string Description { get; set; }
        public String Start { get; set; }
        public String End { get; set; }
        public string Notes { get; set; }
        public String Created { get; set; }
        public String Modified { get; set; }
        public int CustID { get; set; }
        public int? CalendarId { get; set; }
        public string EmployeeName { get; set; }
        public decimal? Price { get; set; }
        public int? Duration { get; set; }
        public int? AttendeesNumber { get; set; }
        public int? ProfileId { get; set; }
        public string CalendarName { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string ServiceName { get; set; }
        public string Street { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public int PaymentMethod { get; set; }


        public NonKuyamAppointmentDetail()
        {
            
        }

        public NonKuyamAppointmentDetail(NonKuyamAppointment item)
        {
            this.AppointmentID = item.AppointmentID;
            this.AppointmentStatusID = item.AppointmentStatusID;
            this.AttendeesNumber = item.AttendeesNumber;
            this.CalendarId = item.CalendarId;
            this.Created = item.Created.ToString("yyyy-MM-dd HH:mm:ss");
            this.CustID = item.CustID;
            this.Description = item.Description;
            this.Duration = item.Duration;
            this.EmployeeName = item.EmployeeName;
            this.End = item.End.ToString("yyyy-MM-dd HH:mm:ss");
            this.Modified = item.Modified.HasValue? item.Modified.Value.ToString("yyyy-MM-dd HH:mm:ss"):"";
            this.Notes = item.Notes;
            this.Price = item.Price;
            this.ServiceId = item.ServiceId;
            this.Start = item.Start.ToString("yyyy-MM-dd HH:mm:ss");
            this.StatusChangeDate = item.StatusChangeDate.HasValue? item.StatusChangeDate.Value.ToString("yyyy-MM-dd HH:mm:ss"):"";
            this.ServiceName = item.Service!=null? item.Service.ServiceName:null;
            this.CalendarName = item.Calendar != null ? item.Calendar.Name : null;
            this.ProfileId = item.ProfileId;
            if (item.ProfileCompany != null)
            {
                this.CompanyName = item.ProfileCompany.Name;
                this.CompanyAddress = item.ProfileCompany.Address;
                this.Street = item.ProfileCompany.Street1;
                this.City = item.ProfileCompany.City;
                this.State = item.ProfileCompany.State;
                this.Zip = item.ProfileCompany.Zip;
                this.PaymentMethod = item.ProfileCompany.PaymentMethod.HasValue?item.ProfileCompany.PaymentMethod.Value:0;
            }

        }

        public NonKuyamAppointmentDetail(ProposedAppointment item)
        {
            this.AppointmentID = item.AppointmentID;
            this.AppointmentStatusID = item.AppointmentStatusID;
            this.AttendeesNumber = item.AttendeesNumber;
            this.CalendarId = item.CalendarId;
            this.Created = item.Created.ToString("yyyy-MM-dd HH:mm:ss");
            this.CustID = item.CustID;
            this.Description = item.Description;
            this.Duration = item.Duration;
            this.EmployeeName = item.EmployeeName;
            this.End = item.End.ToString("yyyy-MM-dd HH:mm:ss");
            this.Modified = item.Modified.HasValue ? item.Modified.Value.ToString("yyyy-MM-dd HH:mm:ss") : "";
            this.Notes = item.Notes;
            this.Price = item.Price;
            this.ServiceId = item.ServiceCompanyId;
            this.Start = item.Start.ToString("yyyy-MM-dd HH:mm:ss");
            this.StatusChangeDate = item.StatusChangeDate.HasValue ? item.StatusChangeDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "";
            this.ServiceName = item.ServiceCompany.Service.ServiceName;
            this.CalendarName = item.Calendar != null ? item.Calendar.Name : null;
            this.ProfileId = item.ProfileId;
            if (item.ProfileCompany != null)
            {
                this.CompanyName = item.ProfileCompany.Name;
                this.CompanyAddress = item.ProfileCompany.Address;
                this.Street = item.ProfileCompany.Street1;
                this.City = item.ProfileCompany.City;
                this.State = item.ProfileCompany.State;
                this.Zip = item.ProfileCompany.Zip;
                this.PaymentMethod = item.ProfileCompany.PaymentMethod.HasValue ? item.ProfileCompany.PaymentMethod.Value : 0;
            }

        }


    }
}
