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
    public partial class be_PostMedia
    {
        public int Id { get; set; }
        public int PostID { get; set; }
        public int MediaType { get; set; }
        public Nullable<int> Crop_x { get; set; }
        public Nullable<int> Crop_y { get; set; }
        public Nullable<int> Rel_width { get; set; }
        public Nullable<int> Rel_height { get; set; }
        public Nullable<int> Fr_width { get; set; }
        public Nullable<int> Fr_height { get; set; }
        public Nullable<int> MediaID { get; set; }
        public Nullable<double> ZoomPercent { get; set; }
    }
    
}
