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
    public class BookingsController : Controller
    {
        private SkyExamsEntities db = new SkyExamsEntities();

        // GET: Bookings
        public ActionResult Index()
        {
            return View(db.Bookings.ToList());
        }

        public ActionResult studentCheck(int? id)
        {
            Sys_User temp = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
            int role = (int)temp.User_Role_ID;
            if (role == 1)
            {
                return RedirectToAction("bookingsScreen", new { id = id });
            }
            else
            {
                return RedirectToAction("slotsScreen", new { id = id });
            }
        }// student check

        [HttpGet]
        public ActionResult slotsScreen(int? id)
        {
            Instructor tempIns = db.Instructors.ToList().Find(i => i.SysUser_ID == id);
            List <Instructor_Slots> slotsList = db.Instructor_Slots.ToList().FindAll(s => s.Instructor_ID == tempIns.Instructor_ID);
            ViewData["userId"] = "" + id;
            return View(slotsList);
        }// slots screen

        [HttpGet]
        public ActionResult createSlot(int? id)
        {
            Instructor tempIns = db.Instructors.ToList().Find(i => i.SysUser_ID == id);
            return View(tempIns);
        }// create slot get

        [HttpPost]
        public ActionResult createSlot(int? id, DateTime slotTime)
        {
            Instructor_Slots newSlot = new Instructor_Slots();
            Instructor tempIns = db.Instructors.ToList().Find(i => i.SysUser_ID == id);
            newSlot.Instructor_ID = tempIns.Instructor_ID;
            newSlot.Date_Time = slotTime;
            newSlot.Booked = false;
            db.Instructor_Slots.Add(newSlot);
            db.SaveChanges();

            return RedirectToAction("slotsScreen", new { id = id });
        }// create slot post

        [HttpGet]
        public ActionResult deleteSlot(int? id, int? slotId)
        {
            ViewData["uID"] = "" + id;
            Instructor_Slots delSlot = db.Instructor_Slots.ToList().Find(s => s.Slot_ID == slotId);
            return View(delSlot);
        }// delete slot get

        public ActionResult deleteSlotConformation(int? id, int? slotId)
        {
            Instructor_Slots delSlot = db.Instructor_Slots.ToList().Find(s => s.Slot_ID == slotId);
            db.Instructor_Slots.Remove(delSlot);
            db.SaveChanges();
            Booking delBooking = db.Bookings.ToList().Find(b => b.Slot_ID == slotId);
            if (delBooking != null)
            {
                db.Bookings.Remove(delBooking);
                db.SaveChangesAsync();
            }//if statement
            return RedirectToAction("slotsScreen", new { id = id });
        }// delete slot conformation

        [HttpGet]
        public ActionResult updateSlot(int? id, int? slotId)
        {
            ViewData["uID"] = "" + id;
            Instructor_Slots updateSlot = db.Instructor_Slots.ToList().Find(s => s.Slot_ID == slotId);
            return View(updateSlot);
        }// update slot get

        [HttpPost]
        public ActionResult updateSlot(int? id, int?slotId, DateTime slotTime)
        {
            Instructor_Slots tempSlot = db.Instructor_Slots.ToList().Find(s => s.Slot_ID == slotId);
            Instructor_Slots updateSlot = new Instructor_Slots();
            updateSlot.Instructor_ID = tempSlot.Instructor_ID;
            updateSlot.Date_Time = slotTime;
            updateSlot.Booked = false;

            db.Instructor_Slots.Remove(tempSlot);
            db.SaveChanges();

            db.Instructor_Slots.Add(updateSlot);
            db.SaveChanges();

            Booking tempBooking = db.Bookings.ToList().Find(b => b.Slot_ID == slotId);
            Booking updateBooking = new Booking();
            if (tempSlot.Booked == true)
            {
                updateBooking.Student_ID = tempBooking.Student_ID;
                updateBooking.Instructor_ID = tempBooking.Instructor_ID;
                updateBooking.Slot_ID = updateSlot.Slot_ID;
                updateBooking.Date_Time = Convert.ToDateTime(updateSlot.Date_Time);
                updateSlot.Booked = true;

                db.Bookings.Remove(tempBooking);
                db.SaveChanges();

                db.Bookings.Add(updateBooking);
                db.SaveChanges();
            }// if statement

            return RedirectToAction("slotsScreen", new { id = id });
        }// update slot post

        public ActionResult bookingsScreen(int? id)
        {
            Student tempStu = db.Students.ToList().Find(s => s.SysUser_ID == id);
            List<Booking> bookingsList = db.Bookings.ToList().FindAll(b => b.Student_ID == tempStu.Student_ID);
            ViewData["uID"] = "" + id;
            return View(bookingsList);
        }// bookings screen

        public ActionResult viewSlots(int? id)
        {
            Student sForId = db.Students.ToList().Find(s => s.SysUser_ID == id);
            Student_Instructor stuIns = db.Student_Instructor.ToList().Find(s => s.Student_ID == sForId.Student_ID);
            List<Instructor_Slots> slots = db.Instructor_Slots.ToList().FindAll(i => i.Instructor_ID == stuIns.Instructor_ID && i.Booked == false);
            ViewData["uID"] = "" + id;
            return View(slots);
        }// view slots

        [HttpGet]
        public ActionResult bookSlot(int? id, int? slotId)
        {
            Instructor_Slots bookSlot = db.Instructor_Slots.ToList().Find(i => i.Slot_ID == slotId);
            Student sForId = db.Students.ToList().Find(s => s.SysUser_ID == id);
            Booking newBooking = new Booking();
            newBooking.Student_ID = sForId.Student_ID;
            newBooking.Instructor_ID = bookSlot.Instructor_ID;
            //newBooking.Slot_ID = bookSlot.Slot_ID + db.Instructor_Slots.ToList().Count;
            newBooking.Date_Time = Convert.ToDateTime(bookSlot.Date_Time);

            Instructor_Slots updateSlot = new Instructor_Slots();
            updateSlot.Instructor_ID = bookSlot.Instructor_ID;
            updateSlot.Date_Time = bookSlot.Date_Time;
            updateSlot.Booked = true;
            db.Instructor_Slots.Remove(bookSlot);
            db.SaveChanges();

            db.Instructor_Slots.Add(updateSlot);
            db.SaveChanges();

            newBooking.Slot_ID = updateSlot.Slot_ID;
            db.Bookings.Add(newBooking);
            db.SaveChanges();

            // send emails

            return RedirectToAction("bookingsScreen", new { id = id });
        }// book slot

        public ActionResult deleteBooking(int? id, int? bookingId)
        {
            ViewData["uID"] = "" + id;
            Booking delBooking = db.Bookings.ToList().Find(b => b.Booking_ID == bookingId);
            return View(delBooking);
        }// delete slot 

        public ActionResult deleteBookingConformation(int? id, int? bookingId)
        {
            Booking delBooking = db.Bookings.ToList().Find(b => b.Booking_ID == bookingId);
            db.Bookings.Remove(delBooking);
            db.SaveChanges();

            Instructor_Slots bookSlot = db.Instructor_Slots.ToList().Find(i => i.Slot_ID == delBooking.Slot_ID);
            Instructor_Slots updateSlot = new Instructor_Slots();
            updateSlot.Instructor_ID = bookSlot.Instructor_ID;
            updateSlot.Date_Time = bookSlot.Date_Time;
            updateSlot.Booked = false;
            db.Instructor_Slots.Remove(bookSlot);
            db.SaveChanges();

            db.Instructor_Slots.Add(updateSlot);
            db.SaveChanges();
            return RedirectToAction("bookingsScreen", new { id = id });
        }// delete booking conformation

        [HttpGet]
        public ActionResult updateBooking(int? id, int? bookingId)
        {
            Student sForId = db.Students.ToList().Find(s => s.SysUser_ID == id);
            Student_Instructor stuIns = db.Student_Instructor.ToList().Find(s => s.Student_ID == sForId.Student_ID);
            List<Instructor_Slots> slots = db.Instructor_Slots.ToList().FindAll(i => i.Instructor_ID == stuIns.Instructor_ID && i.Booked == false);
            ViewData["uID"] = "" + id;
            ViewData["bID"] = "" + bookingId;
            return View(slots);
        }// update booking get

        [HttpGet]
        public ActionResult updateBookingConformation(int? id, int? bookingId, int? slotId)
        {
            Booking tempBooking = db.Bookings.ToList().Find(b => b.Booking_ID == bookingId);
            Instructor_Slots updateSlot = db.Instructor_Slots.ToList().Find(i => i.Slot_ID == slotId);
            Instructor_Slots tempSlot = db.Instructor_Slots.ToList().Find(i => i.Slot_ID == tempBooking.Slot_ID);
            Booking updateBooking = new Booking();

            updateBooking.Student_ID = tempBooking.Student_ID;
            updateBooking.Instructor_ID = tempBooking.Instructor_ID;
            //updateBooking.Slot_ID = tempSlot.Slot_ID + db.Instructor_Slots.ToList().Count;
            updateBooking.Date_Time = Convert.ToDateTime(updateSlot.Date_Time);

            tempSlot.Booked = false;
            updateSlot.Booked = true;
            Instructor_Slots nSlot = tempSlot;
            Instructor_Slots nSlotUpdate = updateSlot;

            db.Instructor_Slots.Remove(tempSlot);
            db.SaveChanges();

            db.Instructor_Slots.Add(nSlot);
            db.SaveChanges();

            db.Instructor_Slots.Remove(updateSlot);
            db.SaveChanges();

            db.Instructor_Slots.Add(nSlotUpdate);
            db.SaveChanges();

            db.Bookings.Remove(tempBooking);
            db.SaveChanges();

            updateBooking.Slot_ID = nSlotUpdate.Slot_ID;
            db.Bookings.Add(updateBooking);
            db.SaveChanges();

            return RedirectToAction("bookingsScreen", new { id = id });
        }// update booking post

        // GET: Bookings/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);
        }

        // GET: Bookings/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Bookings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Booking_ID,B_Description,Date_Time,Student_ID,Instructor_ID,Status_ID")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                db.Bookings.Add(booking);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(booking);
        }

        // GET: Bookings/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);
        }

        // POST: Bookings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Booking_ID,B_Description,Date_Time,Student_ID,Instructor_ID,Status_ID")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                db.Entry(booking).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Booking booking = db.Bookings.Find(id);
            db.Bookings.Remove(booking);
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
