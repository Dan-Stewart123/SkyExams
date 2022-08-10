using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SkyExams.Models;

namespace SkyExams.ViewModels
{
    public class groupStudentVM
    {
        public ICollection<StudentVM> students { get; set; }
        public int totHours;
    }
}