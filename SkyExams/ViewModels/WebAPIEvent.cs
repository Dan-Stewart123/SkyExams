////using DHTMLX.Scheduler.GoogleCalendar;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using SkyExams.Models;

//namespace SkyExams.ViewModels
//{
//    public class WebAPIEvent
//    {
//        public int id { get; set; }
//        public string text { get; set; }
//        public string start_date { get; set; }
//        public string end_date { get; set; }

        public static explicit operator WebAPIEvent(uEvent schedulerEvent)
        {
            return new WebAPIEvent
            {
                id = Convert.ToInt32(schedulerEvent.Event_ID),
                text = schedulerEvent.text,
                //start_date = schedulerEvent.DateTimeScheduled.ToString("yyyy-MM-dd HH:mm"),
                
            };
        }// explicit operator

        public static explicit operator uEvent(WebAPIEvent schedulerEvent)
        {
            return new uEvent
            {
                Event_ID = schedulerEvent.id,
                text = schedulerEvent.text,
                Start_Time = DateTime.Parse(
                    schedulerEvent.start_date,
                    System.Globalization.CultureInfo.InvariantCulture),
                End_Time = DateTime.Parse(
                    schedulerEvent.end_date,
                    System.Globalization.CultureInfo.InvariantCulture)
            };
        }// scheduler event
    }
}