using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SkyExams.ViewModels
{
    public class AnswerVM
    {
        public int QuestionID { get; set; }
        public string QuestionText { get; set; }
        public int AnswerID { get; set; }
        public string AnswerQ { get; set; }
        public bool isCorrect { get; set; }
    }
}