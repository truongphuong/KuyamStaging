using System;
using Kuyam.Database;

namespace Kuyam.Domain.Models
{
    public class RatingModel
    {
        /// <summary>
        /// Map to RatingModel
        /// </summary>
        /// <param name="rating">Rating and include Cust</param>
        public RatingModel(Rating rating)
        {
            Id = rating.ID;
            CustId = rating.CustID;
            EmployeeId = rating.EmployeeID;
            ServiceCompanyId = rating.ServiceCompanyID;
            Content = rating.Content;
            RatingValue = rating.RatingValue == null?0:rating.RatingValue.Value;
            CreateDate = rating.CreateDate;
            PrivateContent = rating.PrivateContent;
        }
        public int Id { get; set; }
        public int CustId { get; set; }
        public int? EmployeeId { get; set; }
        public int ServiceCompanyId { get; set; }
        public string Content { get; set; }
        public int RatingValue { get; set; }
        public DateTime? CreateDate { get; set; }        
        public string PrivateContent { get; set; }
        public CustomerModel Customer { get; set; }
    }    
}
