//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Kuyam.Database
{
    public partial class CategoryFeaturedHotel
    {
        public int Id { get; set; }
        public int FeaturedId { get; set; }
        public int BeCategoryId { get; set; }
        public System.DateTime Created { get; set; }
        public int HotelId { get; set; }
    
        public virtual be_Categories be_Categories { get; set; }
        public virtual FeaturedHotel FeaturedHotel { get; set; }
    }
    
}
