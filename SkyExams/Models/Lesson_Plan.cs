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
    
    public partial class Lesson_Plan
    {
        public int Lesson_Plan_ID { get; set; }
        public int Instructor_ID { get; set; }
        public string LP_Name { get; set; }
        public byte[] LP_Description { get; set; }
        public int Rating_ID { get; set; }
    }
}
