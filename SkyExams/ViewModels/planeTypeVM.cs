using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SkyExams.ViewModels
{
    public class planeTypeVM
    {
        public int Plane_Type_ID { get; set; }
        public string Type_Description { get; set; }
        public byte[] Plane_Image { get; set; }
    }
}