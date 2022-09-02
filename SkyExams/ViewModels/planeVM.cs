using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SkyExams.Models;

namespace SkyExams.ViewModels
{
    public class planeVM
    {
        public int Plane_Type_ID { get; set; }
        public string Type_Description { get; set; }
        public int Plane_ID { get; set; }
        public string Call_Sign { get; set; }
        public string Description { get; set; }
        public int planeHours { get; set; }
        public ICollection<Plane_Service> services { get; set; }
    }
}