using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SkyExams.Models;

namespace SkyExams.ViewModels
{
    public class StudentVM
    {
        public string title { get; set; }
        public string name { get; set; }
        public string licence { get; set; }
        public string hoursFlown { get; set; }
        public ICollection<ExamAverageVM> examAverages { get; set; }
    }
}