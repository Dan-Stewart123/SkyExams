using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SkyExams.Models;

namespace SkyExams.ViewModels
{
    public class QuestionVM
    {
        public int QuestionID { get; set; }
        public string QuestionText { get; set; }
        public ICollection<Answer> Answers { get; set; }
    }
}