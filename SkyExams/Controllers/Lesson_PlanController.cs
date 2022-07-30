using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SkyExams.Models;
using System.IO;

namespace SkyExams.Controllers
{
    public class Lesson_PlanController : Controller
    {
        private SkyExamsEntities db = new SkyExamsEntities();

        // GET: Lesson_Plan
        public ActionResult Index()
        {
            return View(db.Lesson_Plan.ToList());
        }

        public ActionResult planScreen(int? id)
        {
            ViewData["userID"] = "" + id;
            Sys_User forRole = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
            ViewData["userRole"] = "" + forRole.User_Role_ID;
            List<Plane_Type> planeTypes = db.Plane_Type.ToList();

            return View(planeTypes);
        }// returns lesson plan screen

        public FileContentResult getImg(int id)
        {
            byte[] byteArray = db.Plane_Type.Find(id).Plane_Image;
            return byteArray != null
                ? new FileContentResult(byteArray, "image/jpeg")
                : null;
        }

        public ActionResult topicScreen(int? id, int? topicId)
        {
            ViewData["userID"] = "" + id;
            Sys_User user = db.Sys_User.Find(id);
            ViewData["userRole"] = "" + user.User_Role_ID;
            int userRole = Convert.ToInt32(user.User_Role_ID);
            List<Lesson_Plan> planList = new List<Lesson_Plan>();
            if(userRole == 1)
            {
                ViewData["topicId"] = "" + topicId;
                ViewData["planeType"] = db.Plane_Type.ToList().Find(p => p.Plane_Type_ID == topicId).Type_Description;
                Student tempStu = db.Students.ToList().Find(s => s.SysUser_ID == user.SysUser_ID);
                List<Student_Lesson_Plan> stuPlan = db.Student_Lesson_Plan.ToList().FindAll(r => r.Student_ID == tempStu.Student_ID);
                foreach (Student_Lesson_Plan temp in stuPlan)
                {
                    List<Lesson_Plan> tempPlans = db.Lesson_Plan.ToList();
                    foreach (Lesson_Plan temPlan in tempPlans)
                    {
                        if (temPlan.Lesson_Plan_ID == temp.Lesson_Plan_ID && temPlan.Rating_ID == topicId)
                        {
                            planList.Add(temPlan);
                        }
                    }// inner for each
                }// for each
                return View(planList);
                
            }// if user is a student
            else
            {
                ViewData["topicID"] = "" + 1;
                ViewData["planeType"] = db.Plane_Type.ToList().Find(p => p.Plane_Type_ID == topicId).Type_Description;
                planList = db.Lesson_Plan.ToList().FindAll(p => p.Rating_ID == topicId);
                return View(planList);
                if (topicId == 1)
                {
                    ViewData["topicID"] = "" + 1;
                    ViewData["planeType"] = "Cessna 172";
                    planList = db.Lesson_Plan.ToList().FindAll(p => p.Rating_ID == 1);
                    return View(planList);
                }// cessna 172
                if (topicId == 2)
                {
                    ViewData["topicID"] = "" + 2;
                    ViewData["planeType"] = "Cessna 172 RG";
                    planList = db.Lesson_Plan.ToList().FindAll(p => p.Rating_ID == 2);
                    return View(planList);
                }// cessna 172 RG
                if (topicId == 3)
                {
                    ViewData["topicID"] = "" + 3;
                    ViewData["planeType"] = "Cherokee 140";
                    planList = db.Lesson_Plan.ToList().FindAll(p => p.Rating_ID == 3);
                    return View(planList);
                }// cherokee 140
                if (topicId == 4)
                {
                    ViewData["topicID"] = "" + 4;
                    ViewData["planeType"] = "Piper Twin Comanche";
                    planList = db.Lesson_Plan.ToList().FindAll(p => p.Rating_ID == 4);
                    return View(planList);
                }// twin commanche
                else
                {
                    return View(planList);
                }
            }// else
        }// theme screen

        [HttpGet]
        public FileResult downloadPlan(int? id)
        {
            Lesson_Plan downloadPlan = db.Lesson_Plan.Find(id);
            var file = downloadPlan.LP_Description;
            return File(file, "application/pdf");
        }// download file

        public ActionResult addPlan(int? id, int? topicId)
        {
            ViewData["topicID"] = "" + topicId;
            Sys_User user = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
            return View(user);
        }// add resource screen get

        [HttpPost]
        public ActionResult addPlan(int? id, int? topicId, string name, HttpPostedFileBase plan)
        {
            List<Lesson_Plan> planList = db.Lesson_Plan.ToList();
            if (name == "" || plan == null)
            {
                return RedirectToAction("addPlan", new { id = id, topicId = topicId });
            }// if fields are empty
            else
            {
                int resourceId = planList.Count + 3;
                Lesson_Plan newPlan = new Lesson_Plan();
                newPlan.Lesson_Plan_ID = resourceId;
                newPlan.LP_Name = name;
                int theme = Convert.ToInt32(topicId);
                newPlan.Rating_ID = theme;
                int instructorId = db.Instructors.ToList().Find(i => i.SysUser_ID == id).Instructor_ID;
                newPlan.Instructor_ID = instructorId;

                Stream str = plan.InputStream;
                BinaryReader br = new BinaryReader(str);
                Byte[] fileDetails = br.ReadBytes((Int32)str.Length);
                newPlan.LP_Description = fileDetails;

                db.Lesson_Plan.Add(newPlan);
                db.SaveChanges();
                Instructor tempInstructor = db.Instructors.ToList().Find(i => i.SysUser_ID == id);
                List<Student_Instructor> studentInstructor = db.Student_Instructor.ToList().FindAll(i => i.Instructor_ID == tempInstructor.Instructor_ID);
                foreach(Student_Instructor temp in studentInstructor)
                {
                    Student tempStudent = db.Students.ToList().Find(s => s.Student_ID == temp.Student_ID);
                    Sys_User tempUser = db.Sys_User.ToList().Find(u => u.SysUser_ID == tempStudent.SysUser_ID);
                    Student_Lesson_Plan newStuPlan = new Student_Lesson_Plan();
                    newStuPlan.Student_ID = tempStudent.Student_ID;
                    newStuPlan.Lesson_Plan_ID = newPlan.Lesson_Plan_ID;
                    db.Student_Lesson_Plan.Add(newStuPlan);
                    db.SaveChangesAsync();
                }// for each
                return RedirectToAction("planScreen", new { id = id });
            }// else
        }// add resource post

        public ActionResult deletePlan(int? loggedId, int? id)
        {
            ViewData["loggedId"] = "" + loggedId;
            Lesson_Plan delPlan = db.Lesson_Plan.ToList().Find(p => p.Lesson_Plan_ID == id);
            return View(delPlan);
        }// delete Resource

        public ActionResult deletePlanConformation(int? loggedId, int? id)
        {
            Lesson_Plan delPlan = db.Lesson_Plan.Find(id);
            db.Lesson_Plan.Remove(delPlan);
            db.SaveChanges();
            Student_Lesson_Plan delStuPlan = db.Student_Lesson_Plan.ToList().Find(p => p.Lesson_Plan_ID == delPlan.Lesson_Plan_ID);
            db.Student_Lesson_Plan.Remove(delStuPlan);
            db.SaveChanges();
            return RedirectToAction("planScreen", new { id = loggedId });
        }// delete conformation

        // GET: Lesson_Plan/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Lesson_Plan lesson_Plan = db.Lesson_Plan.Find(id);
            if (lesson_Plan == null)
            {
                return HttpNotFound();
            }
            return View(lesson_Plan);
        }

        // GET: Lesson_Plan/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Lesson_Plan/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Lesson_Plan_ID,Instructor_ID,LP_Description,Topic_ID")] Lesson_Plan lesson_Plan)
        {
            if (ModelState.IsValid)
            {
                db.Lesson_Plan.Add(lesson_Plan);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(lesson_Plan);
        }

        // GET: Lesson_Plan/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Lesson_Plan lesson_Plan = db.Lesson_Plan.Find(id);
            if (lesson_Plan == null)
            {
                return HttpNotFound();
            }
            return View(lesson_Plan);
        }

        // POST: Lesson_Plan/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Lesson_Plan_ID,Instructor_ID,LP_Description,Topic_ID")] Lesson_Plan lesson_Plan)
        {
            if (ModelState.IsValid)
            {
                db.Entry(lesson_Plan).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(lesson_Plan);
        }

        // GET: Lesson_Plan/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Lesson_Plan lesson_Plan = db.Lesson_Plan.Find(id);
            if (lesson_Plan == null)
            {
                return HttpNotFound();
            }
            return View(lesson_Plan);
        }

        // POST: Lesson_Plan/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Lesson_Plan lesson_Plan = db.Lesson_Plan.Find(id);
            db.Lesson_Plan.Remove(lesson_Plan);
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
