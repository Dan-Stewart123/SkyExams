using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
//using DHTMLX.Common;
//using DHTMLX.Scheduler;
//using DHTMLX.Scheduler.Data;
using SkyExams.Models;

namespace SkyExams.Controllers
{

    public class CalendarActionResponseModel
    {
        public String Status;
        public Int64 Source_id;
        public Int64 Target_id;

        public CalendarActionResponseModel(String status, Int64 source_id, Int64 target_id)
        {
            Status = status;
            Source_id = source_id;
            Target_id = target_id;
        }
    }


    public class uEventsController : Controller
    {
        private SkyExamsEntities db = new SkyExamsEntities();

        public ActionResult viewEvents(int? id)
        {
            ViewData["userId"] = "" + id;

            return View();

        }// view events

        //public ContentResult Data()
        //{
        //    try
        //    {
        //        var details = db.uEvents.ToList();



        //        return new SchedulerAjaxData(details);

        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }

        //}// get events from db

        //public ContentResult Save(int? id, FormCollection actionValues)
        //{

        //    var action = new DataAction(actionValues);

        //    try
        //    {
        //        var changedEvent = (uEvent)DHXEventsHelper.Bind(typeof(uEvent), actionValues);



        //        switch (action.Type)
        //        {
        //            case DataActionTypes.Insert:
        //                uEvent EV = new uEvent();
        //                EV.Event_ID = changedEvent.Event_ID;
        //                EV.Start_Time = changedEvent.Start_Time;
        //                EV.End_Time = changedEvent.End_Time;
        //                EV.text = changedEvent.text;
        //                EV.Event_Type_ID = 1;
        //                db.uEvents.Add(EV);
        //                db.SaveChanges();


        //                break;
        //            case DataActionTypes.Delete:
        //                var details = db.uEvents.Where(x => x.Event_ID == id).FirstOrDefault();
        //                db.uEvents.Remove(details);
        //                db.SaveChanges();

        //                break;
        //            default:// "update"    
        //                var data = db.uEvents.Where(x => x.Event_ID == id).FirstOrDefault();
        //                data.Start_Time = changedEvent.Start_Time;
        //                data.End_Time = changedEvent.End_Time;
        //                data.text = changedEvent.text;
        //                data.Event_Type_ID = 1;
        //                db.SaveChanges();


        //                break;
        //        }
        //    }
        //    catch
        //    {
        //        action.Type = DataActionTypes.Error;
        //    }
        //    return (ContentResult)new AjaxSaveResponse(action);
        //}





        // GET: uEvents
        public ActionResult Index()
        {
            return View(db.uEvents.ToList());
        }

        // GET: uEvents/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            uEvent uEvent = db.uEvents.Find(id);
            if (uEvent == null)
            {
                return HttpNotFound();
            }
            return View(uEvent);
        }

        // GET: uEvents/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: uEvents/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Event_ID,Start_Time,End_Time,Location_ID,Event_Type_ID")] uEvent uEvent)
        {
            if (ModelState.IsValid)
            {
                db.uEvents.Add(uEvent);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(uEvent);
        }

        // GET: uEvents/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            uEvent uEvent = db.uEvents.Find(id);
            if (uEvent == null)
            {
                return HttpNotFound();
            }
            return View(uEvent);
        }

        // POST: uEvents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Event_ID,Start_Time,End_Time,Location_ID,Event_Type_ID")] uEvent uEvent)
        {
            if (ModelState.IsValid)
            {
                db.Entry(uEvent).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(uEvent);
        }

        // GET: uEvents/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            uEvent uEvent = db.uEvents.Find(id);
            if (uEvent == null)
            {
                return HttpNotFound();
            }
            return View(uEvent);
        }

        // POST: uEvents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            uEvent uEvent = db.uEvents.Find(id);
            db.uEvents.Remove(uEvent);
            db.SaveChanges();
            return RedirectToAction("Index");
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
