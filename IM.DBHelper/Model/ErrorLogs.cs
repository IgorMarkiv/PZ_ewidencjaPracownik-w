//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DBHelper.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class ErrorLogs
    {
        public int id { get; set; }
        public string errorName { get; set; }
        public string errorValue { get; set; }
        public string errorCode { get; set; }
        public Nullable<System.DateTime> DateTime { get; set; }
    }
}
