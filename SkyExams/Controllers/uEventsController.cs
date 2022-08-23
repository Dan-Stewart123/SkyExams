using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SkyExams.Models;

namespace SkyExams.Controllers
{
    public class uEventsController : Controller
    {
        private SkyExamsEntities db = new SkyExamsEntities();

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
        public ActionResult Create([Bind(Include = "Event_ID,Date_Time,Location_ID,Event_Type_ID")] uEvent uEvent)
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
        public ActionResult Edit([Bind(Include = "Event_ID,Date_Time,Location_ID,Event_Type_ID")] uEvent uEvent)
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
