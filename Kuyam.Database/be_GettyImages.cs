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
    public partial class be_GettyImages
    {
        public int Id { get; set; }
        public string LocationData { get; set; }
        public string Title { get; set; }
        public string UrlThumb { get; set; }
        public string UrlPreview { get; set; }
        public Nullable<int> Status { get; set; }
        public Nullable<int> PixelHeight { get; set; }
        public Nullable<int> PixelWidth { get; set; }
        public string UrlAttachment { get; set; }
        public string Tags { get; set; }
        public int UserID { get; set; }
        public string GettyImageId { get; set; }
    
        public virtual be_Users be_Users { get; set; }
    }
    
}
