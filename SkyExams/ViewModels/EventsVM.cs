using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SkyExams.ViewModels
{
    public class EventsVM
    {
        public ulong startTime { get; set; }
        public ulong endTime { get; set; }
        public string location { get; set; }
        public string type { get; set; }
    }
}