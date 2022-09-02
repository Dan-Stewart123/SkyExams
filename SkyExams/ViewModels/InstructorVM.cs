using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SkyExams.ViewModels;

namespace SkyExams.ViewModels
{
    public class InstructorVM
    {
        public string title { get; set; }
        public string name { get; set; }
        public int students { get; set; }
        public int studentHours { get; set; }
        public string licence { get; set; }
        public ICollection<ExamAverageVM> examAverages { get; set; }
    }
}