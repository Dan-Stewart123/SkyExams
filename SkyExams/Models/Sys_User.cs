//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SkyExams.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Sys_User
    {
        public int SysUser_ID { get; set; }
        public Nullable<int> User_Role_ID { get; set; }
        public int Title_ID { get; set; }
        public string FName { get; set; }
        public string Surname { get; set; }
        public string Cell_Number { get; set; }
        public string Email_Address { get; set; }
        public string Physical_Address { get; set; }
        public Nullable<System.DateTime> DOB { get; set; }
        public int Password_ID { get; set; }
        public int City_ID { get; set; }
        public int Country_ID { get; set; }
        public int ZIP_ID { get; set; }
        public int Employment_ID { get; set; }
        public string User_Name { get; set; }
    }
}
