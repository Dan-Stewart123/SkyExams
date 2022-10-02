using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SkyExams.Models;
using SkyExams.ViewModels;


namespace SkyExams.Controllers 
{

    public class uEventsController : Controller
    {
        private SkyExamsEntities db = new SkyExamsEntities();

        public ActionResult Index(int? id)
        {
            try
            {
                if (id != null)
                {
                    Sys_User tempUser = db.Sys_User.ToList().Find(s => s.SysUser_ID == Convert.ToInt32(id));
                    ViewData["userId"] = "" + tempUser.SysUser_ID;
                    ViewData["role"] = "" + tempUser.User_Role_ID;
                    return View();
                }
                else
                {
                    return RedirectToAction("loginScreen", "Sys_User");
                }
            }
            catch
            {
                return RedirectToAction("loginScreen", "Sys_User");
            }

        }

        public JsonResult GetEvents()
        {
            using (var ecm = new SkyExamsEntities())
            {
                //System.Diagnostics.Debug.WriteLine("get events");         
                var events = ecm.uEvents.ToList();
                /*foreach(var singleEvent in events)
                {
                    System.Diagnostics.Debug.WriteLine(singleEvent.EventID);
                    System.Diagnostics.Debug.WriteLine(singleEvent.Subject);
                    System.Diagnostics.Debug.WriteLine(singleEvent.Description);
                    System.Diagnostics.Debug.WriteLine(singleEvent.Start);
                    System.Diagnostics.Debug.WriteLine(singleEvent.End);
                }*/
                return new JsonResult { Data = events, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        // create or update for CRUD
        [HttpPost]
        public JsonResult SaveEvent(uEvent t)
        {
            var status = false;
            try
            {
                using (SkyExamsEntities ecm = new SkyExamsEntities())
                {
                    //System.Diagnostics.Debug.WriteLine("event id is: " + e.EventID);
                    // if >0, then this entry is already in our db
                    if (t.Event_ID > 0) // 0 is the default, which is not allowed in our db
                    {
                        //Update the event
                        var entry = ecm.uEvents.Where(e => e.Event_ID == t.Event_ID).FirstOrDefault();
                        if (entry != null)
                        {
                            entry.text = t.text;
                            entry.Start = t.Start;
                            entry.End = t.End;
                            entry.Event_Type_ID = t.Event_Type_ID;
                            entry.Location_ID = t.Location_ID;
                        }
                        // else, something unexpected occured.
                    }
                    else
                    {
                        t.Event_ID = 1;
                        // if table is not empty
                        if (ecm.uEvents.Any())
                        {
                            t.Event_ID = ecm.uEvents.Max(e => e.Event_ID) + 1;
                        }

                        ecm.uEvents.Add(t);

                        /* System.Diagnostics.Debug.WriteLine("start is: " + e.Start.ToString());
                         System.Diagnostics.Debug.WriteLine("end is: " + e.End.ToString());*/
                    }

                    ecm.SaveChanges();
                    status = true;
                }
            }
            catch (Exception ex) //Catch Other Exception
            {
                System.Diagnostics.Debug.WriteLine("Exception message is: " + ex.ToString());
            }

            return new JsonResult { Data = new { status = status } };
        }

        // delete for CRUD
        [HttpPost]
        public JsonResult DeleteEvent(uEvent t)
        {
            var status = false;
            //System.Diagnostics.Debug.WriteLine("event ID to delete is: "+ e.EventID);
            try
            {
                using (var ecm = new SkyExamsEntities())
                {
                    var entry = ecm.uEvents.Where(e => e.Start == t.Start && e.End == t.End && e.text == t.text).FirstOrDefault();
                    if (entry != null)
                    {
                        ecm.uEvents.Remove(entry);
                        ecm.SaveChanges();
                        status = true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Delete exception message is: " + ex.ToString());
            }

            return new JsonResult { Data = new { status = status } };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
