using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SkyExams.Models;
using SkyExams.ViewModels;

namespace SkyExams.Controllers
{
    public class PlanesController : Controller
    {
        private SkyExamsEntities db = new SkyExamsEntities();

        // GET: Planes
        public ActionResult Index()
        {
            return View(db.Planes.ToList());
        }

        public ActionResult planesScreen(int? id)
        {
            ViewData["userID"] = "" + id;
            Sys_User forRole = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
            ViewData["userRole"] = "" + forRole.User_Role_ID;
            List<Plane_Type> planeTypes = db.Plane_Type.ToList();
            List<planeTypeVM> planeTypeView = new List<planeTypeVM>();
            foreach(var p in planeTypes)
            {
                planeTypeVM temp = new planeTypeVM();
                temp.Plane_Type_ID = p.Plane_Type_ID;
                temp.Type_Description = p.Type_Description;
                temp.Plane_Image = p.Plane_Image;
            }// for each
            return View(planeTypes);
        }// returns plane screen

        public FileContentResult getImg(int id)
        {
            byte[] byteArray = db.Plane_Type.Find(id).Plane_Image;
            return byteArray != null
                ? new FileContentResult(byteArray, "image/jpeg")
                : null;
        }

        public ActionResult addPlaneScreen(int? id)
        {
            ViewData["userID"] = "" + id;
            List <Plane_Type> typeList = db.Plane_Type.ToList();
            return View(typeList);
        }// get for add plane screen

        [HttpPost]
        public ActionResult addPlaneScreen(int id, string type, string description, string sign, int? hoursFlown, int? serviceHours)
        {
            if(type == "" || description == "" || sign == "" || hoursFlown == 0 || serviceHours == 0)
            {
                return RedirectToAction("addPlaneScreen", new { id = id });
            }// checks fields arent empty
            else
            {
                Plane newPlane = new Plane();
                List<Plane> planeList = new List<Plane>();
                int planeID = planeList.Count() + 2;
                newPlane.Plane_ID = planeID;
                newPlane.Call_Sign = sign;
                newPlane.Hours_Flown = hoursFlown;
                newPlane.Hours_Until_Service = serviceHours;
                newPlane.Description = description;
                newPlane.In_Service = false;
                newPlane.Plane_Type_ID = Convert.ToInt32(type);
                
                db.Planes.Add(newPlane);
                db.SaveChanges();
                Sys_User user = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
                return RedirectToAction("planesScreen", new { id = id });
            }// if fields are valid
        }// post for add plane screen

        public ActionResult planeTypeScreen(int? id, int? typeId)
        {
            ViewData["userID"] = "" + id;
            Sys_User forRole = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
            ViewData["userRole"] = "" + forRole.User_Role_ID;
            List<Plane> planeList = new List<Plane>();
            ViewData["planeType"] = db.Plane_Type.ToList().Find(p => p.Plane_Type_ID == typeId).Type_Description;
            planeList = db.Planes.ToList().FindAll(p => p.Plane_Type_ID == typeId);
            return View(planeList);
        }// plane type screen

        public ActionResult deletePlane(int? loggedId, int? id)
        {
            ViewData["loggedId"] = "" + loggedId;
            //ViewData["planeType"] = db.Planes.ToList().Find(p => p.Plane_ID == id);
            Plane delPlane = db.Planes.ToList().Find(p => p.Plane_ID == id);
            ViewData["planeType"] = db.Plane_Type.ToList().Find(p => p.Plane_Type_ID == delPlane.Plane_Type_ID).Type_Description;
            return View(delPlane);
        }// delete plane

        public ActionResult deletePlaneConformation(int? loggedId, int? id)
        {
            Plane delPlane = db.Planes.Find(id);
            int planeType = delPlane.Plane_Type_ID;
            int planeID = delPlane.Plane_ID;
            db.Planes.Remove(delPlane);
            db.SaveChanges();
            
            return RedirectToAction("planeTypeScreen", new { id = loggedId, typeId = planeType });
        }// delete plane conformation

        public ActionResult updatePlane(int? loggedId, int? id)
        {
            ViewData["loggedId"] = "" + loggedId;
            Plane updatePlane = db.Planes.ToList().Find(p => p.Plane_ID == id);
            return View(updatePlane);
        }// update plane get

        [HttpPost]
        public ActionResult updatePlane(string userId, string id, string description, string sign, int? hoursFlown, int? serviceHours)
        {
            int pID = Convert.ToInt32(id);
            Plane plane = db.Planes.Find(pID);
            if(description == "" || sign == "" || hoursFlown == 0 || serviceHours == 0)
            {
                return RedirectToAction("updatePlane");
            }// checks if all fields are complete 
            else
            {
                Plane updatePlane = new Plane();
                updatePlane.Plane_ID = plane.Plane_ID;
                updatePlane.Plane_Type_ID = plane.Plane_Type_ID;
                updatePlane.Call_Sign = sign;
                updatePlane.Hours_Flown = hoursFlown;
                updatePlane.Hours_Until_Service = serviceHours;
                updatePlane.Description = description;
                updatePlane.In_Service = plane.In_Service;
                

                db.Planes.Remove(plane);
                db.SaveChanges();

                db.Planes.Add(updatePlane);
                db.SaveChanges();

                int uID = Convert.ToInt32(userId);
                return RedirectToAction("planeTypeScreen", new { id = uID, typeId = updatePlane.Plane_Type_ID });
            }// else
        }// update plane post

        public ActionResult updatePlaneHours(int? loggedId, int? id)
        {
            ViewData["loggedId"] = "" + loggedId;
            Plane updatePlane = db.Planes.ToList().Find(p => p.Plane_ID == id);
            return View(updatePlane);
        }// update plane hours get

        [HttpPost]
        public ActionResult updatePlaneHours(string userId, string id, int? hoursFlown)
        {
            int pID = Convert.ToInt32(id);
            Plane plane = db.Planes.Find(pID);
            if (hoursFlown == 0)
            {
                return RedirectToAction("updatePlane");
            }// checks if all fields are complete 
            else
            {
                Plane updatePlane = new Plane();
                updatePlane.Plane_ID = plane.Plane_ID;
                updatePlane.Plane_Type_ID = plane.Plane_Type_ID;
                updatePlane.Call_Sign = plane.Call_Sign;
                updatePlane.Hours_Flown = hoursFlown;
                updatePlane.Hours_Until_Service = plane.Hours_Until_Service;
                updatePlane.Description = plane.Description;
                updatePlane.In_Service = plane.In_Service;

                //if for email 

                db.Planes.Remove(plane);
                db.SaveChanges();

                updatePlane.Hours_Until_Service = updatePlane.Hours_Until_Service - (updatePlane.Hours_Flown - plane.Hours_Flown);
                db.Planes.Add(updatePlane);
                db.SaveChanges();

                int uID = Convert.ToInt32(userId);
                return RedirectToAction("planeTypeScreen", new { id = uID, typeId = updatePlane.Plane_Type_ID });
            }// else
        }// update plane post

        public ActionResult serviceCheck(int? loggedId, int? id)
        {
            Plane checkPlane = db.Planes.ToList().Find(p => p.Plane_ID == id);
            if(checkPlane.In_Service == false)
            {
                return RedirectToAction("bookPlaneOut", new { loggedId = loggedId, id = checkPlane.Plane_ID });
            }// false
            else
            {
                return RedirectToAction("captureServiceDetails", new { loggedId = loggedId, id = checkPlane.Plane_ID });
            }// true
        }// service check

        public ActionResult bookPlaneOut(int? loggedId, int? id)
        {
            Plane plane = db.Planes.ToList().Find(p => p.Plane_ID == id);
            Plane updatePlane = new Plane();

            updatePlane.Plane_ID = plane.Plane_ID;
            updatePlane.Plane_Type_ID = plane.Plane_Type_ID;
            updatePlane.Call_Sign = plane.Call_Sign;
            updatePlane.Hours_Flown = plane.Hours_Flown;
            updatePlane.Hours_Until_Service = plane.Hours_Until_Service;
            updatePlane.Description = plane.Description;
            updatePlane.In_Service = true;


            db.Planes.Remove(plane);
            db.SaveChanges();

            db.Planes.Add(updatePlane);
            db.SaveChanges();

            return RedirectToAction("planeTypeScreen", new { id = loggedId, typeId = updatePlane.Plane_Type_ID });
        }// book out plane

        public ActionResult captureServiceDetails(int? loggedId, int? id)
        {
            ViewData["loggedId"] = "" + loggedId;
            Plane updatePlane = db.Planes.ToList().Find(p => p.Plane_ID == id);
            return View(updatePlane);
        }// capture service details

        [HttpPost]
        public ActionResult captureServiceDetails(int? loggedId, int? id, string details)
        {
            Plane plane = db.Planes.ToList().Find(p => p.Plane_ID == id);
            Plane updatePlane = new Plane();
            if (details == "")
            {
                return RedirectToAction("captureServiceDetails", new { loggedId = loggedId, id = plane.Plane_ID });
            }// if statement
            else
            {
                

                updatePlane.Plane_ID = plane.Plane_ID;
                updatePlane.Plane_Type_ID = plane.Plane_Type_ID;
                updatePlane.Call_Sign = plane.Call_Sign;
                updatePlane.Hours_Flown = plane.Hours_Flown;
                updatePlane.Hours_Until_Service = 0;
                updatePlane.Description = plane.Description;
                updatePlane.In_Service = false;


                db.Planes.Remove(plane);
                db.SaveChanges();

                db.Planes.Add(updatePlane);
                db.SaveChanges();

                Plane_Service newService = new Plane_Service();
                newService.Plane_ID = updatePlane.Plane_ID;
                newService.Last_Service_Date = DateTime.Now;
                newService.Service_Details = details;

                db.Plane_Service.Add(newService);
                db.SaveChanges();

                return RedirectToAction("planeTypeScreen", new { id = loggedId, typeId = updatePlane.Plane_Type_ID });
            }// else

        }// capture service details post

        [HttpGet]
        public ActionResult addPlaneTypeScreen(int? id)
        {
            Sys_User user = db.Sys_User.ToList().Find(s => s.SysUser_ID == id);
            return View(user);
        }// add plane type get

        [HttpPost]
        public ActionResult addPlaneTypeScreen(int? id, string name, HttpPostedFileBase pic)
        {
            Plane_Type newPlaneType = new Plane_Type();
            newPlaneType.Type_Description = name;

            byte[] imageBytes = null;
            BinaryReader reader = new BinaryReader(pic.InputStream);
            imageBytes = reader.ReadBytes((int)pic.ContentLength);
            newPlaneType.Plane_Image = imageBytes;

            db.Plane_Type.Add(newPlaneType);
            db.SaveChanges();

            Exam newExam = new Exam();
            newExam.Exam_ID = newPlaneType.Plane_Type_ID;
            newExam.Plane_Type_ID = newPlaneType.Plane_Type_ID;

            db.Exams.Add(newExam);
            db.SaveChanges();

            Rating newRating = new Rating();
            newRating.Rating_ID = newPlaneType.Plane_Type_ID;
            newRating.Rating_Description = name;

            db.Ratings.Add(newRating);
            db.SaveChanges();

            Question_Rating newQR = new Question_Rating();
            newQR.Question_Rating_ID = newPlaneType.Plane_Type_ID;
            newQR.Ques_Rating = name;

            db.Question_Rating.Add(newQR);
            db.SaveChanges();

            return RedirectToAction("planesScreen", new { id = id });
        }// add plane type post

        [HttpGet]
        public ActionResult deletePlaneType(int? loggedId, int? id)
        {
            ViewData["loggedId"] = "" + loggedId;
            Plane_Type delPlane = db.Plane_Type.ToList().Find(p => p.Plane_Type_ID == id);
            return View(delPlane);
        }// delete plane type get

        public ActionResult deletePlaneTypeConformation(int? loggedId, int? id)
        {
            Plane_Type delPlane = db.Plane_Type.Find(id);

            List<Plane> planeList = db.Planes.ToList().FindAll(p => p.Plane_Type_ID == delPlane.Plane_Type_ID);
            if(planeList != null)
            {
                foreach(var p in planeList)
                {
                    Plane_Service temp = db.Plane_Service.ToList().Find(s => s.Plane_ID == p.Plane_ID);
                    if(temp != null)
                    {
                        db.Plane_Service.Remove(temp);
                        db.SaveChanges();
                    }// if

                    db.Planes.Remove(p);
                    db.SaveChanges();

                }// for each
            }// plane if

            Exam ex = db.Exams.ToList().Find(e => e.Plane_Type_ID == delPlane.Plane_Type_ID);
            if(ex != null)
            {
                Load_Sheet tempSheet = db.Load_Sheet.ToList().Find(l => l.Exam_ID == ex.Exam_ID);
                Exam_Average tempAverage = db.Exam_Average.ToList().Find(a => a.Exam_ID == ex.Exam_ID);
                if (tempSheet != null)
                {
                    db.Load_Sheet.Remove(tempSheet);
                    db.SaveChanges();
                }// sheet if
                if (tempAverage != null)
                {
                    db.Exam_Average.Remove(tempAverage);
                    db.SaveChanges();
                }// average if

                db.Exams.Remove(ex);
                db.SaveChanges();

            }// exam if

            Rating rate = db.Ratings.ToList().Find(r => r.Rating_ID == delPlane.Plane_Type_ID);
            if(rate != null)
            {
                List <Study_Resource> resourceList = db.Study_Resource.ToList().FindAll(s => s.Rating_ID == rate.Rating_ID);
                foreach(var r in resourceList)
                {
                    Student_Resource temp = db.Student_Resource.ToList().Find(s => s.Study_Resource_ID == r.Study_Resource_ID);

                    db.Student_Resource.Remove(temp);
                    db.SaveChanges();

                    db.Study_Resource.Remove(r);
                    db.SaveChanges();
                }// for each

                List<Lesson_Plan> planList = db.Lesson_Plan.ToList().FindAll(s => s.Rating_ID == rate.Rating_ID);
                foreach (var p in planList)
                {
                    Student_Lesson_Plan temp = db.Student_Lesson_Plan.ToList().Find(s => s.Lesson_Plan_ID == p.Lesson_Plan_ID);

                    db.Student_Lesson_Plan.Remove(temp);
                    db.SaveChanges();

                    db.Lesson_Plan.Remove(p);
                    db.SaveChanges();
                }// for each

                db.Ratings.Remove(rate);
                db.SaveChanges();
            }// rating if

            Question_Rating qRate = db.Question_Rating.ToList().Find(q => q.Question_Rating_ID == delPlane.Plane_Type_ID);
            if(qRate != null)
            {
                List<Question> qList = db.Questions.ToList().FindAll(q => q.Question_Rating_ID == qRate.Question_Rating_ID);
                foreach(var q in qList)
                {
                    List<Answer> answerList = db.Answers.ToList().FindAll(a => a.Question_ID == q.Question_ID);
                    foreach(var a in answerList)
                    {
                        db.Answers.Remove(a);
                        db.SaveChanges();
                    }
                    db.Questions.Remove(q);
                    db.SaveChanges();
                }// for each

                db.Question_Rating.Remove(qRate);
                db.SaveChanges();
            }// question rating if

            db.Plane_Type.Remove(delPlane);
            db.SaveChanges();

            return RedirectToAction("planesScreen", new { id = loggedId });
        }// delete plane type post

        [HttpGet]
        public ActionResult updatePlaneType(int? loggedId, int? id)
        {
            ViewData["loggedId"] = "" + loggedId;
            Plane_Type plane_Type = db.Plane_Type.Find(id);
            return View(plane_Type);
        }// update plane type get

        [HttpPost]
        public ActionResult updatePlaneType(int? loggedId, int? id, string name, HttpPostedFileBase pic)
        {
            Plane_Type updatePType = new Plane_Type();
            updatePType.Plane_Type_ID = Convert.ToInt32(id);
            updatePType.Type_Description = name;

            byte[] imageBytes = null;
            BinaryReader reader = new BinaryReader(pic.InputStream);
            imageBytes = reader.ReadBytes((int)pic.ContentLength);
            updatePType.Plane_Image = imageBytes;

            db.Entry(updatePType).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("planesScreen", new { id = loggedId });
        }// update plane type post

        // GET: Planes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Plane plane = db.Planes.Find(id);
            if (plane == null)
            {
                return HttpNotFound();
            }
            return View(plane);
        }

        // GET: Planes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Planes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Plane_ID,Plane_Type_ID,Call_Sign,Hours_Flown,Hours_Until_Service")] Plane plane)
        {
            if (ModelState.IsValid)
            {
                db.Planes.Add(plane);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(plane);
        }

        // GET: Planes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Plane plane = db.Planes.Find(id);
            if (plane == null)
            {
                return HttpNotFound();
            }
            return View(plane);
        }

        // POST: Planes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Plane_ID,Plane_Type_ID,Call_Sign,Hours_Flown,Hours_Until_Service")] Plane plane)
        {
            if (ModelState.IsValid)
            {
                db.Entry(plane).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(plane);
        }

        // GET: Planes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Plane plane = db.Planes.Find(id);
            if (plane == null)
            {
                return HttpNotFound();
            }
            return View(plane);
        }

        // POST: Planes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Plane plane = db.Planes.Find(id);
            db.Planes.Remove(plane);
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
