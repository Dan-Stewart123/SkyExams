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
using MimeKit;
using MailKit;
using MailKit.Net.Smtp;

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
            try
            {
                if (id != null)
                {
                    ViewData["userID"] = "" + id;
                    Sys_User forRole = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
                    ViewData["userRole"] = "" + forRole.User_Role_ID;
                    List<Plane_Type> planeTypes = db.Plane_Type.ToList();

                    return View(planeTypes);
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

        }// returns plane screen

        public FileContentResult getImg(int id)
        {
            byte[] byteArray = db.Plane_Type.Find(id).Plane_Image;
            return byteArray != null
                ? new FileContentResult(byteArray, "image/jpeg")
                : null;
        }

        public ActionResult addPlaneScreen(int? id, string err)
        {
            try
            {
                if (id != null)
                {
                    ViewData["userID"] = "" + id;
                    ViewData["err"] = err;
                    List<Plane_Type> typeList = db.Plane_Type.ToList();
                    return View(typeList);
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

        }// get for add plane screen

        [HttpPost]
        public ActionResult addPlaneScreen(int id, string type, string description, string sign, int? hoursFlown, int? serviceHours)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    if (type == "" || description == "" || sign == "" || hoursFlown == 0 || serviceHours == 0)
                    {
                        string temp = "Hint: Complete all the fields before clicking submit.";
                        return RedirectToAction("addPlaneScreen", new { id = id, err = temp });
                    }// checks fields arent empty
                    else
                    {
                        Plane newPlane = new Plane();
                        List<Plane> planeList = new List<Plane>();
                        //int planeID = planeList.Count() + 2;
                        //newPlane.Plane_ID = planeID;
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
            
        }// post for add plane screen

        public ActionResult planeTypeScreen(int? id, int? typeId)
        {
            try
            {
                if (id != null || typeId != null)
                {
                    ViewData["userID"] = "" + id;
                    Sys_User forRole = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
                    ViewData["userRole"] = "" + forRole.User_Role_ID;
                    List<Plane> planeList = new List<Plane>();
                    ViewData["planeType"] = db.Plane_Type.ToList().Find(p => p.Plane_Type_ID == typeId).Type_Description;
                    planeList = db.Planes.ToList().FindAll(p => p.Plane_Type_ID == typeId);
                    return View(planeList);
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

        }// plane type screen

        public ActionResult deletePlane(int? loggedId, int? id)
        {
            try
            {
                if (loggedId != null || id != null)
                {
                    ViewData["loggedId"] = "" + loggedId;
                    //ViewData["planeType"] = db.Planes.ToList().Find(p => p.Plane_ID == id);
                    Plane delPlane = db.Planes.ToList().Find(p => p.Plane_ID == id);
                    ViewData["planeType"] = db.Plane_Type.ToList().Find(p => p.Plane_Type_ID == delPlane.Plane_Type_ID).Type_Description;
                    return View(delPlane);
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

        }// delete plane

        public ActionResult deletePlaneConformation(int? loggedId, int? id)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    Plane delPlane = db.Planes.Find(id);
                    int planeType = delPlane.Plane_Type_ID;
                    int planeID = delPlane.Plane_ID;
                    db.Planes.Remove(delPlane);
                    db.SaveChanges();

                    foreach (var ps in db.Plane_Service.ToList().FindAll(p => p.Plane_ID == id))
                    {
                        db.Plane_Service.Remove(ps);
                        db.SaveChanges();
                    }// for each

                    return RedirectToAction("planeTypeScreen", new { id = loggedId, typeId = planeType });
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
            
        }// delete plane conformation

        public ActionResult updatePlane(int? loggedId, int? id, string err)
        {
            try
            {
                if (loggedId != null || id != null)
                {
                    ViewData["loggedId"] = "" + loggedId;
                    Plane updatePlane = db.Planes.ToList().Find(p => p.Plane_ID == id);
                    ViewData["err"] = err;
                    return View(updatePlane);
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

        }// update plane get

        [HttpPost]
        public ActionResult updatePlane(string userId, string id, string description, string sign, int? hoursFlown, int? serviceHours)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    int pID = Convert.ToInt32(id);
                    Plane plane = db.Planes.Find(pID);
                    if (description == "" || sign == "" || hoursFlown == 0 || serviceHours == 0)
                    {
                        string temp = "Hint: Complete all the fields before clicking submit.";
                        return RedirectToAction("updatePlane", new { loggedId = userId, id = id, err = temp });
                    }// checks if all fields are complete 
                    else
                    {
                        Plane updatePlane = new Plane();
                        //updatePlane.Plane_ID = plane.Plane_ID;
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

                        foreach (var ps in db.Plane_Service.ToList().FindAll(p => p.Plane_ID == Convert.ToInt32(id)))
                        {
                            Plane_Service updatePS = new Plane_Service();
                            updatePS = ps;
                            updatePS.Plane_ID = db.Planes.ToList().Find(p => p.Call_Sign == updatePlane.Call_Sign).Plane_ID;

                            db.Plane_Service.Remove(ps);
                            db.SaveChanges();

                            db.Plane_Service.Add(updatePS);
                            db.SaveChanges();
                        }// for each

                        int uID = Convert.ToInt32(userId);
                        return RedirectToAction("planeTypeScreen", new { id = uID, typeId = updatePlane.Plane_Type_ID });
                    }// else
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
            
        }// update plane post

        public ActionResult updatePlaneHours(int? loggedId, int? id, string err)
        {
            try
            {
                if (loggedId != null || id != null)
                {
                    ViewData["loggedId"] = "" + loggedId;
                    ViewData["err"] = err;
                    Plane updatePlane = db.Planes.ToList().Find(p => p.Plane_ID == id);
                    return View(updatePlane);
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

        }// update plane hours get

        [HttpPost]
        public ActionResult updatePlaneHours(string userId, string id, int? hoursFlown)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    int pID = Convert.ToInt32(id);
                    Plane plane = db.Planes.Find(pID);
                    if (hoursFlown == null)
                    {
                        string temp = "Hint: Complete all the fields before clicking submit.";
                        return RedirectToAction("updatePlaneHours", new { loggedId = userId, id = id, err = temp });
                    }// checks if all fields are complete 
                    else
                    {
                        Plane updatePlane = new Plane();
                        //updatePlane.Plane_ID = plane.Plane_ID;
                        updatePlane.Plane_Type_ID = plane.Plane_Type_ID;
                        updatePlane.Call_Sign = plane.Call_Sign;
                        updatePlane.Hours_Flown = hoursFlown;
                        updatePlane.Hours_Until_Service = plane.Hours_Until_Service;
                        updatePlane.Description = plane.Description;
                        updatePlane.In_Service = plane.In_Service;


                        if (updatePlane.Hours_Flown <= 10)
                        {
                            Plane_Type type = db.Plane_Type.ToList().Find(p => p.Plane_Type_ID == updatePlane.Plane_Type_ID);

                            MimeMessage requestEmail = new MimeMessage();
                            requestEmail.From.Add(new MailboxAddress("Booking conformation", "skyexams.fts@gmail.com"));
                            requestEmail.To.Add(MailboxAddress.Parse("danielmarcstewart@gmail.com"));// to admin email
                            requestEmail.Subject = "New user request";
                            requestEmail.Body = new TextPart("plain") { Text = "The following plane is due for its service soon: Call Sign: " + updatePlane.Call_Sign + " Plane type: " + type.Type_Description + " Hours until service: " + updatePlane.Hours_Until_Service };

                            //send email
                            SmtpClient client = new SmtpClient();
                            client.Connect("smtp.gmail.com", 465, true);
                            client.Authenticate("skyexams.fts@gmail.com", "hyekkmqkosqoqmth");
                            client.Send(requestEmail);
                            client.Disconnect(true);
                            client.Dispose();
                        }// if for email

                        db.Planes.Remove(plane);
                        db.SaveChanges();

                        updatePlane.Hours_Until_Service = updatePlane.Hours_Until_Service - (updatePlane.Hours_Flown - plane.Hours_Flown);
                        db.Planes.Add(updatePlane);
                        db.SaveChanges();

                        foreach (var ps in db.Plane_Service.ToList().FindAll(p => p.Plane_ID == Convert.ToInt32(id)))
                        {
                            Plane_Service updatePS = new Plane_Service();
                            updatePS = ps;
                            updatePS.Plane_ID = db.Planes.ToList().Find(p => p.Call_Sign == updatePlane.Call_Sign).Plane_ID;

                            db.Plane_Service.Remove(ps);
                            db.SaveChanges();

                            db.Plane_Service.Add(updatePS);
                            db.SaveChanges();
                        }// for each

                        int uID = Convert.ToInt32(userId);
                        return RedirectToAction("planeTypeScreen", new { id = uID, typeId = updatePlane.Plane_Type_ID });
                    }// else
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
            
        }// update plane post

        public ActionResult serviceCheck(int? loggedId, int? id)
        {
            try
            {
                if (loggedId != null || id != null)
                {
                    Plane checkPlane = db.Planes.ToList().Find(p => p.Plane_ID == id);
                    if (checkPlane.In_Service == false)
                    {
                        return RedirectToAction("bookPlaneOut", new { loggedId = loggedId, id = checkPlane.Plane_ID });
                    }// false
                    else
                    {
                        return RedirectToAction("captureServiceDetails", new { loggedId = loggedId, id = checkPlane.Plane_ID });
                    }// true
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

        }// service check

        public ActionResult bookPlaneOut(int? loggedId, int? id)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    Plane plane = db.Planes.ToList().Find(p => p.Plane_ID == id);
                    Plane updatePlane = new Plane();

                    //updatePlane.Plane_ID = plane.Plane_ID;
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

                    foreach (var ps in db.Plane_Service.ToList().FindAll(p => p.Plane_ID == id))
                    {
                        Plane_Service updatePS = new Plane_Service();
                        updatePS = ps;
                        updatePS.Plane_ID = db.Planes.ToList().Find(p => p.Call_Sign == updatePlane.Call_Sign).Plane_ID;

                        db.Plane_Service.Remove(ps);
                        db.SaveChanges();

                        db.Plane_Service.Add(updatePS);
                        db.SaveChanges();
                    }// for each

                    return RedirectToAction("planeTypeScreen", new { id = loggedId, typeId = updatePlane.Plane_Type_ID });
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

        }// book out plane

        [HttpGet]
        public ActionResult captureServiceDetails(int? loggedId, int? id, string err)
        {
            try
            {
                if (loggedId != null || id != null)
                {
                    ViewData["loggedId"] = "" + loggedId;
                    ViewData["err"] = err;
                    Plane updatePlane = db.Planes.ToList().Find(p => p.Plane_ID == id);
                    return View(updatePlane);
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

        }// capture service details

        [HttpPost]
        public ActionResult captureServiceDetailsConf(int? loggedId, int? id, string details, string parts)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    Plane plane = db.Planes.ToList().Find(p => p.Plane_ID == id);
                    Plane updatePlane = new Plane();
                    if (details == "")
                    {
                        string temp = "Hint: Complete all the fields before clicking submit.";
                        return RedirectToAction("captureServiceDetails", new { loggedId = loggedId, id = plane.Plane_ID, err = temp });
                    }// if statement
                    else
                    {


                        //updatePlane.Plane_ID = plane.Plane_ID;
                        updatePlane.Plane_Type_ID = plane.Plane_Type_ID;
                        updatePlane.Call_Sign = plane.Call_Sign;
                        updatePlane.Hours_Flown = plane.Hours_Flown;
                        updatePlane.Hours_Until_Service = 50;
                        updatePlane.Description = plane.Description;
                        updatePlane.In_Service = false;


                        db.Planes.Remove(plane);// fix plane id for report to get all services, test test test
                        db.SaveChanges();

                        db.Planes.Add(updatePlane);
                        db.SaveChanges();

                        foreach (var ps in db.Plane_Service.ToList().FindAll(p => p.Plane_ID == id))
                        {
                            Plane_Service updatePS = new Plane_Service();
                            updatePS = ps;
                            updatePS.Plane_ID = db.Planes.ToList().Find(p => p.Call_Sign == updatePlane.Call_Sign).Plane_ID;

                            db.Plane_Service.Remove(ps);
                            db.SaveChanges();

                            db.Plane_Service.Add(updatePS);
                            db.SaveChanges();
                        }// for each

                        Plane_Service newService = new Plane_Service();
                        newService.Plane_ID = updatePlane.Plane_ID;
                        newService.Last_Service_Date = DateTime.Now;
                        newService.Reason_For_Service = details;
                        newService.Parts_Used = parts;

                        db.Plane_Service.Add(newService);
                        db.SaveChanges();

                        return RedirectToAction("planeTypeScreen", new { id = loggedId, typeId = updatePlane.Plane_Type_ID });
                    }// else
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
            

        }// capture service details post

        [HttpGet]
        public ActionResult addPlaneTypeScreen(int? id, string err)
        {
            try
            {
                if (id != null)
                {
                    Sys_User user = db.Sys_User.ToList().Find(s => s.SysUser_ID == id);
                    ViewData["err"] = err;
                    return View(user);
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

        }// add plane type get

        [HttpPost]
        public ActionResult addPlaneTypeScreen(int? id, string name, HttpPostedFileBase pic)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    if(name == "" || pic == null)
                    {
                        string temp = "Hint: Complete all the fields before clicking submit.";
                        return RedirectToAction("addPlaneTypeScreen", new { id = id, err = temp });
                    }
                    else
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
                        //newExam.Exam_ID = newPlaneType.Plane_Type_ID;
                        newExam.Plane_Type_ID = newPlaneType.Plane_Type_ID;

                        db.Exams.Add(newExam);
                        db.SaveChanges();

                        Rating newRating = new Rating();
                        //newRating.Rating_ID = newPlaneType.Plane_Type_ID;
                        newRating.Rating_Description = name;

                        db.Ratings.Add(newRating);
                        db.SaveChanges();

                        Question_Rating newQR = new Question_Rating();
                        //newQR.Question_Rating_ID = newPlaneType.Plane_Type_ID;
                        newQR.Ques_Rating = name;

                        db.Question_Rating.Add(newQR);
                        db.SaveChanges();

                        return RedirectToAction("planesScreen", new { id = id });
                    }
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
            
        }// add plane type post

        [HttpGet]
        public ActionResult deletePlaneType(int? loggedId, int? id)
        {
            try
            {
                if (loggedId != null || id != null)
                {
                    ViewData["loggedId"] = "" + loggedId;
                    Plane_Type delPlane = db.Plane_Type.ToList().Find(p => p.Plane_Type_ID == id);
                    return View(delPlane);
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

        }// delete plane type get

        public ActionResult deletePlaneTypeConformation(int? loggedId, int? id)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    Plane_Type delPlane = db.Plane_Type.Find(id);

                    List<Plane> planeList = db.Planes.ToList().FindAll(p => p.Plane_Type_ID == delPlane.Plane_Type_ID);
                    if (planeList != null)
                    {
                        foreach (var p in planeList)
                        {
                            Plane_Service temp = db.Plane_Service.ToList().Find(s => s.Plane_ID == p.Plane_ID);
                            if (temp != null)
                            {
                                db.Plane_Service.Remove(temp);
                                db.SaveChanges();
                            }// if

                            db.Planes.Remove(p);
                            db.SaveChanges();

                        }// for each
                    }// plane if

                    Exam ex = db.Exams.ToList().Find(e => e.Plane_Type_ID == delPlane.Plane_Type_ID);
                    if (ex != null)
                    {
                        Load_Sheet tempSheet = db.Load_Sheet.ToList().Find(l => l.Exam_ID == ex.Exam_ID);
                        Exam_Average tempAverage = db.Exam_Average.ToList().Find(a => a.Exam_ID == ex.Exam_ID);
                        List<Student_Exam> stuExams = db.Student_Exam.ToList().FindAll(s => s.Exam_ID == ex.Exam_ID);
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
                        if (stuExams != null)
                        {
                            foreach (var se in stuExams)
                            {
                                db.Student_Exam.Remove(se);
                                db.SaveChanges();
                            }
                        }

                        db.Exams.Remove(ex);
                        db.SaveChanges();

                    }// exam if

                    Rating rate = db.Ratings.ToList().Find(r => r.Rating_ID == delPlane.Plane_Type_ID);
                    if (rate != null)
                    {
                        List<Study_Resource> resourceList = db.Study_Resource.ToList().FindAll(s => s.Rating_ID == rate.Rating_ID);
                        if (resourceList != null)
                        {
                            foreach (var r in resourceList)
                            {
                                Student_Resource temp = db.Student_Resource.ToList().Find(s => s.Study_Resource_ID == r.Study_Resource_ID);

                                if (temp != null)
                                {
                                    db.Student_Resource.Remove(temp);
                                    db.SaveChanges();
                                }// if statement

                                db.Study_Resource.Remove(r);
                                db.SaveChanges();
                            }// for each
                        }// if statement

                        List<Lesson_Plan> planList = db.Lesson_Plan.ToList().FindAll(s => s.Rating_ID == rate.Rating_ID);
                        if (planList != null)
                        {
                            foreach (var p in planList)
                            {
                                Student_Lesson_Plan temp = db.Student_Lesson_Plan.ToList().Find(s => s.Lesson_Plan_ID == p.Lesson_Plan_ID);

                                db.Student_Lesson_Plan.Remove(temp);
                                db.SaveChanges();

                                db.Lesson_Plan.Remove(p);
                                db.SaveChanges();
                            }// for each
                        }// if statement

                        db.Ratings.Remove(rate);
                        db.SaveChanges();
                    }// rating if

                    Question_Rating qRate = db.Question_Rating.ToList().Find(q => q.Question_Rating_ID == delPlane.Plane_Type_ID);
                    if (qRate != null)
                    {
                        List<Question> qList = db.Questions.ToList().FindAll(q => q.Question_Rating_ID == qRate.Question_Rating_ID);
                        foreach (var q in qList)
                        {
                            List<Answer> answerList = db.Answers.ToList().FindAll(a => a.Question_ID == q.Question_ID);
                            foreach (var a in answerList)
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
            
        }// delete plane type post

        [HttpGet]
        public ActionResult updatePlaneType(int? loggedId, int? id, string err)
        {
            try
            {
                if (loggedId != null || id != null)
                {
                    ViewData["loggedId"] = "" + loggedId;
                    ViewData["err"] = err;
                    Plane_Type plane_Type = db.Plane_Type.Find(id);
                    return View(plane_Type);
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

        }// update plane type get

        [HttpPost]
        public ActionResult updatePlaneType(int? loggedId, int? id, string name, HttpPostedFileBase pic)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    if (name == "" || pic == null)
                    {
                        string temp = "Hint: Complete all the fields before clicking submit.";
                        return RedirectToAction("updatePlaneType", new { loggedId = loggedId, id = id, err = temp });
                    }
                    else
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
                    }
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
