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
    
    public partial class Student_Exam
    {
        public int Student_ID { get; set; }
        public int Exam_ID { get; set; }
        public Nullable<int> Exam_Mark { get; set; }
        public Nullable<bool> Started { get; set; }
        public bool Completed { get; set; }
        public int Stu_Exam { get; set; }
        public Nullable<System.DateTime> Date_Completed { get; set; }
    }
}
