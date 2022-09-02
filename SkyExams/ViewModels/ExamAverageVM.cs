using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SkyExams.ViewModels
{
    public class ExamAverageVM
    {
        public int examId { get; set; }
        public string examName { get; set; }
        public int examAvg { get; set; }
    }
}