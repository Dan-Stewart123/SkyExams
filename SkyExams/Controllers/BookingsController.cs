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
using MimeKit;
using MailKit;
using MailKit.Net.Smtp;

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
            try
            {
                if (id != null)
                {
                    Instructor tempIns = db.Instructors.ToList().Find(i => i.SysUser_ID == id);
                    List<Instructor_Slots> slotsList = db.Instructor_Slots.ToList().FindAll(s => s.Instructor_ID == tempIns.Instructor_ID);
                    ViewData["userId"] = "" + id;
                    return View(slotsList);
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

        }// slots screen

        [HttpGet]
        public ActionResult createSlot(int? id, string err)
        {
            try
            {
                if (id != null)
                {
                    Instructor tempIns = db.Instructors.ToList().Find(i => i.SysUser_ID == id);
                    ViewData["err"] = err;
                    return View(tempIns);
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

        }// create slot get

        [HttpPost]
        public ActionResult createSlot(int? id, DateTime slotTime)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    if(slotTime == null)
                    {
                        string temp = "Hint: Complete all the fields before clicking submit.";
                        return RedirectToAction("createSlot", new { id = id, err = temp });
                    }
                    Instructor_Slots newSlot = new Instructor_Slots();
                    Instructor tempIns = db.Instructors.ToList().Find(i => i.SysUser_ID == id);
                    newSlot.Instructor_ID = tempIns.Instructor_ID;
                    newSlot.Date_Time = slotTime;
                    newSlot.Date_Time_String = "" + slotTime;
                    newSlot.Booked = false;
                    List<Instructor_Slots> slotsList = db.Instructor_Slots.ToList().FindAll(s => s.Instructor_ID == tempIns.Instructor_ID);
                    bool check = false;
                    foreach(var slots in slotsList)
                    {
                        if(slots.Date_Time == newSlot.Date_Time)
                        {
                            check = true;
                        }// if statement
                    }// for each
                    if(check == false)
                    {
                        db.Instructor_Slots.Add(newSlot);
                        db.SaveChanges();
                    }// if statement
                    
                    return RedirectToAction("slotsScreen", new { id = id });
                }
                else
                {
                    return RedirectToAction("loginScreen");
                }
            }
            catch
            {
                return RedirectToAction("loginScreen");
            }
            
        }// create slot post

        [HttpGet]
        public ActionResult deleteSlot(int? id, int? slotId)
        {
            try
            {
                if (id != null || slotId != null)
                {
                    ViewData["uID"] = "" + id;
                    Instructor_Slots delSlot = db.Instructor_Slots.ToList().Find(s => s.Slot_ID == slotId);
                    return View(delSlot);
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

        }// delete slot get

        public ActionResult deleteSlotConformation(int? id, int? slotId)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    Instructor_Slots delSlot = db.Instructor_Slots.ToList().Find(s => s.Slot_ID == slotId);
                    db.Instructor_Slots.Remove(delSlot);
                    db.SaveChanges();
                    Booking delBooking = db.Bookings.ToList().Find(b => b.Slot_ID == slotId);
                    if (delBooking != null)
                    {
                        Student tempStu = db.Students.ToList().Find(s => s.Student_ID == delBooking.Student_ID);
                        Sys_User tempUser = db.Sys_User.ToList().Find(u => u.SysUser_ID == tempStu.SysUser_ID);
                        db.Bookings.Remove(delBooking);
                        db.SaveChangesAsync();                      

                        try
                        {
                            //create email
                            MimeMessage requestEmail = new MimeMessage();
                            requestEmail.From.Add(new MailboxAddress("Booking Cancellation", "skyexams.fts@gmail.com"));
                            requestEmail.To.Add(MailboxAddress.Parse("danielmarcstewart@gmail.com"));// to student
                            requestEmail.Subject = "Booking Cancellation";
                            requestEmail.Body = new TextPart("plain") { Text = "Your slot on " + delSlot.Date_Time + " has been cancelled by  your instructor" };

                            //send email
                            SmtpClient client = new SmtpClient();
                            client.Connect("smtp.gmail.com", 465, true);
                            client.Authenticate("skyexams.fts@gmail.com", "hyekkmqkosqoqmth");
                            client.Send(requestEmail);
                            client.Disconnect(true);
                            client.Dispose();
                        }// try
                        catch
                        {
                            return RedirectToAction("bookingsScreen", new { id = id });
                        }// catch
                    }//if statement
                    return RedirectToAction("slotsScreen", new { id = id });
                }
                else
                {
                    return RedirectToAction("loginScreen");
                }
            }
            catch
            {
                return RedirectToAction("loginScreen");
            }
            
        }// delete slot conformation

        [HttpGet]
        public ActionResult updateSlot(int? id, int? slotId)
        {
            try
            {
                if (id != null || slotId != null)
                {
                    ViewData["uID"] = "" + id;
                    Instructor_Slots updateSlot = db.Instructor_Slots.ToList().Find(s => s.Slot_ID == slotId);
                    return View(updateSlot);
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

        }// update slot get

        [HttpPost]
        public ActionResult updateSlot(int? id, int?slotId, DateTime slotTime)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    Instructor_Slots tempSlot = db.Instructor_Slots.ToList().Find(s => s.Slot_ID == slotId);
                    Instructor_Slots updateSlot = new Instructor_Slots();
                    updateSlot.Instructor_ID = tempSlot.Instructor_ID;
                    updateSlot.Date_Time = slotTime;
                    updateSlot.Date_Time_String = "" + slotTime;
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

                        try
                        {
                            //create email
                            MimeMessage requestEmail = new MimeMessage();
                            requestEmail.From.Add(new MailboxAddress("Booking Change", "skyexams.fts@gmail.com"));
                            requestEmail.To.Add(MailboxAddress.Parse("danielmarcstewart@gmail.com"));// to student
                            requestEmail.Subject = "Booking Change";
                            requestEmail.Body = new TextPart("plain") { Text = "Your slot on " + tempSlot.Date_Time + " has been changed to " + updateSlot.Date_Time + " by  your instructor" };

                            //send email
                            SmtpClient client = new SmtpClient();
                            client.Connect("smtp.gmail.com", 465, true);
                            client.Authenticate("skyexams.fts@gmail.com", "hyekkmqkosqoqmth");
                            client.Send(requestEmail);
                            client.Disconnect(true);
                            client.Dispose();
                        }// try
                        catch
                        {
                            return RedirectToAction("bookingsScreen", new { id = id });
                        }// catch

                    }// if statement

                    return RedirectToAction("slotsScreen", new { id = id });
                }
                else
                {
                    return RedirectToAction("loginScreen");
                }
            }
            catch
            {
                return RedirectToAction("loginScreen");
            }
            
        }// update slot post

        public ActionResult bookingsScreen(int? id)
        {
            try
            {
                if (id != null)
                {
                    Student tempStu = db.Students.ToList().Find(s => s.SysUser_ID == id);
                    List<Booking> bookingsList = db.Bookings.ToList().FindAll(b => b.Student_ID == tempStu.Student_ID);
                    ViewData["uID"] = "" + id;
                    return View(bookingsList);
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

        }// bookings screen

        public ActionResult viewSlots(int? id)
        {
            try
            {
                if (id != null)
                {
                    Student sForId = db.Students.ToList().Find(s => s.SysUser_ID == id);
                    Student_Instructor stuIns = db.Student_Instructor.ToList().Find(s => s.Student_ID == sForId.Student_ID);
                    List<Sys_User> instructors = db.Sys_User.ToList().FindAll(i => i.User_Role_ID == 2);
                    List<Plane_Type> types = new List<Plane_Type>();
                    List<SelectListItem> planeTypes = new List<SelectListItem>();
                    List<Student_Exam> sExam = db.Student_Exam.ToList().FindAll(s => s.Student_ID == sForId.Student_ID);
                    foreach (var e in sExam)
                    {
                        Plane_Type temp = db.Plane_Type.ToList().Find(p => p.Plane_Type_ID == e.Exam_ID);
                        types.Add(temp);
                    }// for each
                    foreach (var p in types)
                    {
                        SelectListItem temp = new SelectListItem();
                        temp.Value = "" + p.Plane_Type_ID;
                        temp.Text = p.Type_Description;
                        planeTypes.Add(temp);
                    }// for each
                    ViewBag.planes = planeTypes;
                    ViewData["uID"] = "" + id;
                    return View(instructors);
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

        }// view slots

        public JsonResult getSlots(int instructor)
        {
            Instructor tempIns = db.Instructors.ToList().Find(i => i.SysUser_ID == instructor);
            var jsonData = db.Instructor_Slots.ToList().FindAll(s => s.Instructor_ID == tempIns.Instructor_ID && s.Booked == false);
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }// get slots JSON

        public ActionResult bookSlot(int? id, int? instructors, int? planes, int? FromJson)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    Instructor_Slots bookSlot = db.Instructor_Slots.ToList().Find(i => i.Slot_ID == FromJson);
                    Student sForId = db.Students.ToList().Find(s => s.SysUser_ID == id);
                    Booking newBooking = new Booking();
                    newBooking.Student_ID = sForId.Student_ID;
                    newBooking.Instructor_ID = bookSlot.Instructor_ID;
                    //newBooking.Slot_ID = bookSlot.Slot_ID + db.Instructor_Slots.ToList().Count;
                    newBooking.Date_Time = Convert.ToDateTime(bookSlot.Date_Time);
                    newBooking.Plane_Type_ID = Convert.ToInt32(planes);

                    Instructor_Slots updateSlot = new Instructor_Slots();
                    updateSlot.Instructor_ID = bookSlot.Instructor_ID;
                    updateSlot.Date_Time = bookSlot.Date_Time;
                    updateSlot.Booked = true;
                    updateSlot.Date_Time_String = bookSlot.Date_Time_String;
                    
                    db.Entry(updateSlot).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    newBooking.Slot_ID = updateSlot.Slot_ID;
                    db.Bookings.Add(newBooking);
                    db.SaveChanges();

                    Sys_User stu = db.Sys_User.ToList().Find(s => s.SysUser_ID == sForId.SysUser_ID);

                    try
                    {
                        //create email
                        MimeMessage requestEmail = new MimeMessage();
                        requestEmail.From.Add(new MailboxAddress("Booking conformation", "skyexams.fts@gmail.com"));
                        requestEmail.To.Add(MailboxAddress.Parse("danielmarcstewart@gmail.com"));// to instructor
                        requestEmail.Subject = "Booking Conformation";
                        requestEmail.Body = new TextPart("plain") { Text = "Your slot on " + updateSlot.Date_Time + " has been booked by " + stu.FName + " " + stu.Surname };

                        //send email
                        SmtpClient client = new SmtpClient();
                        client.Connect("smtp.gmail.com", 465, true);
                        client.Authenticate("skyexams.fts@gmail.com", "hyekkmqkosqoqmth");
                        client.Send(requestEmail);
                        client.Disconnect(true);
                        client.Dispose();
                    }// try
                    catch
                    {
                        return RedirectToAction("bookingsScreen", new { id = id });
                    }// catch

                    return RedirectToAction("bookingsScreen", new { id = id });
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

        }// book slot

        public ActionResult deleteBooking(int? id, int? bookingId)
        {
            try
            {
                if (id != null || bookingId != null)
                {
                    ViewData["uID"] = "" + id;
                    Booking delBooking = db.Bookings.ToList().Find(b => b.Booking_ID == bookingId);
                    Instructor tempIns = db.Instructors.ToList().Find(i => i.Instructor_ID == delBooking.Instructor_ID);
                    ViewData["ins"] = db.Sys_User.ToList().Find(s => s.SysUser_ID == tempIns.SysUser_ID).FName + " " + db.Sys_User.ToList().Find(s => s.SysUser_ID == tempIns.SysUser_ID).Surname;
                    return View(delBooking);
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

        }// delete slot 

        public ActionResult deleteBookingConformation(int? id, int? bookingId)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    Booking delBooking = db.Bookings.ToList().Find(b => b.Booking_ID == bookingId);
                    db.Bookings.Remove(delBooking);
                    db.SaveChanges();

                    Instructor_Slots bookSlot = db.Instructor_Slots.ToList().Find(i => i.Slot_ID == delBooking.Slot_ID);
                    Instructor_Slots updateSlot = new Instructor_Slots();
                    updateSlot.Instructor_ID = bookSlot.Instructor_ID;
                    updateSlot.Date_Time = bookSlot.Date_Time;
                    updateSlot.Date_Time_String = bookSlot.Date_Time_String;
                    updateSlot.Booked = false;
                    db.Instructor_Slots.Remove(bookSlot);
                    db.SaveChanges();

                    db.Instructor_Slots.Add(updateSlot);
                    db.SaveChanges();

                    try
                    {
                        //create email
                        MimeMessage requestEmail = new MimeMessage();
                        requestEmail.From.Add(new MailboxAddress("Booking Cancellation", "skyexams.fts@gmail.com"));
                        requestEmail.To.Add(MailboxAddress.Parse("danielmarcstewart@gmail.com"));// to instructor
                        requestEmail.Subject = "Booking Cancellation";
                        requestEmail.Body = new TextPart("plain") { Text = "Your slot on " + bookSlot.Date_Time + " has been cancelled by the student" };

                        //send email
                        SmtpClient client = new SmtpClient();
                        client.Connect("smtp.gmail.com", 465, true);
                        client.Authenticate("skyexams.fts@gmail.com", "hyekkmqkosqoqmth");
                        client.Send(requestEmail);
                        client.Disconnect(true);
                        client.Dispose();
                    }// try
                    catch
                    {
                        return RedirectToAction("bookingsScreen", new { id = id });
                    }// catch

                    return RedirectToAction("bookingsScreen", new { id = id });
                }
                else
                {
                    return RedirectToAction("loginScreen");
                }
            }
            catch
            {
                return RedirectToAction("loginScreen");
            }
            
        }// delete booking conformation

        [HttpGet]
        public ActionResult updateBooking(int? id, int? bookingId)
        {
            try
            {
                if (id != null || bookingId != null)
                {
                    Student sForId = db.Students.ToList().Find(s => s.SysUser_ID == id);
                    Student_Instructor stuIns = db.Student_Instructor.ToList().Find(s => s.Student_ID == sForId.Student_ID);
                    List<Instructor_Slots> slots = db.Instructor_Slots.ToList().FindAll(i => i.Instructor_ID == stuIns.Instructor_ID && i.Booked == false);
                    ViewData["uID"] = "" + id;
                    ViewData["bID"] = "" + bookingId;
                    Instructor tempIns = db.Instructors.ToList().Find(i => i.Instructor_ID == stuIns.Instructor_ID);
                    ViewData["ins"] = db.Sys_User.ToList().Find(s => s.SysUser_ID == tempIns.SysUser_ID).FName + " " + db.Sys_User.ToList().Find(s => s.SysUser_ID == tempIns.SysUser_ID).Surname;
                    return View(slots);
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

        }// update booking get

        [HttpGet]
        public ActionResult updateBookingConformation(int? id, int? bookingId, int? slotId)
        {
            try
            {
                if (id != null || bookingId != null || slotId != null)
                {
                    Booking tempBooking = db.Bookings.ToList().Find(b => b.Booking_ID == bookingId);
                    Instructor_Slots updateSlot = db.Instructor_Slots.ToList().Find(i => i.Slot_ID == slotId);
                    Instructor_Slots tempSlot = db.Instructor_Slots.ToList().Find(i => i.Slot_ID == tempBooking.Slot_ID);
                    Booking updateBooking = new Booking();

                    updateBooking.Student_ID = tempBooking.Student_ID;
                    updateBooking.Instructor_ID = tempBooking.Instructor_ID;
                    //updateBooking.Slot_ID = tempSlot.Slot_ID + db.Instructor_Slots.ToList().Count;
                    updateBooking.Date_Time = Convert.ToDateTime(updateSlot.Date_Time);
                    updateBooking.Plane_Type_ID = tempBooking.Plane_Type_ID;

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

                    try
                    {
                        //create email
                        MimeMessage requestEmail = new MimeMessage();
                        requestEmail.From.Add(new MailboxAddress("Booking Change", "skyexams.fts@gmail.com"));
                        requestEmail.To.Add(MailboxAddress.Parse("danielmarcstewart@gmail.com"));// to instructor
                        requestEmail.Subject = "Booking Change";
                        requestEmail.Body = new TextPart("plain") { Text = "Your slot on " + tempSlot.Date_Time + " has been changed to " + updateSlot.Date_Time + "  by the student" };

                        //send email
                        SmtpClient client = new SmtpClient();
                        client.Connect("smtp.gmail.com", 465, true);
                        client.Authenticate("skyexams.fts@gmail.com", "hyekkmqkosqoqmth");
                        client.Send(requestEmail);
                        client.Disconnect(true);
                        client.Dispose();
                    }// try
                    catch
                    {
                        return RedirectToAction("bookingsScreen", new { id = id });
                    }// catch

                    return RedirectToAction("bookingsScreen", new { id = id });
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
