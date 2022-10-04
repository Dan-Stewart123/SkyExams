using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using SkyExams.Models;
using SkyExams.ViewModels;

namespace SkyExams.Controllers
{
    public class ReportsController : Controller
    {

        private SkyExamsEntities db = new SkyExamsEntities();

        public void SetPageCacheNoStore()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.AppendCacheExtension("no-store, must-revalidate");
            Response.AppendHeader("Pragma", "no-cache");
            Response.AppendHeader("Expires", "0");
        }

        public ActionResult reportsScreen(int? id)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    Sys_User user = db.Sys_User.ToList().Find(s => s.SysUser_ID == id);
                    SetPageCacheNoStore();
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

        }// reports screen

        public ActionResult studentReport(int? id)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    ViewData["userID"] = "" + id;
                    List<Sys_User> students = db.Sys_User.ToList().FindAll(s => s.User_Role_ID == 1);
                    SetPageCacheNoStore();
                    return View(students);
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

        }// student report

        public ActionResult generateStudentReport(int? loggedId, int? id)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    Sys_User user = db.Sys_User.ToList().Find(s => s.SysUser_ID == id);
                    Student stu = db.Students.ToList().Find(s => s.SysUser_ID == user.SysUser_ID);
                    StudentVM reportStu = new StudentVM();

                    reportStu.title = db.Titles.ToList().Find(t => t.Title_ID == user.Title_ID).TitleDesc;
                    reportStu.name = user.FName + " " + user.Surname;
                    reportStu.licence = "" + stu.Licence_No;
                    reportStu.hoursFlown = "" + stu.Hours_Flown;
                    List<ExamAverageVM> avgs = new List<ExamAverageVM>();
                    List<Student_Exam> sExams = db.Student_Exam.ToList().FindAll(e => e.Student_ID == stu.Student_ID);
                    foreach (var s in sExams)
                    {

                        ExamAverageVM avg = new ExamAverageVM();
                        avg.examId = s.Exam_ID;
                        avg.examName = db.Plane_Type.ToList().Find(p => p.Plane_Type_ID == s.Exam_ID).Type_Description;
                        avg.examAvg = Convert.ToInt32(s.Exam_Mark);
                        avgs.Add(avg);

                    }// for each

                    reportStu.examAverages = avgs;

                    ViewData["userID"] = "" + loggedId;
                    ViewData["userName"] = db.Sys_User.ToList().Find(s => s.SysUser_ID == loggedId).FName + " " + db.Sys_User.ToList().Find(s => s.SysUser_ID == loggedId).Surname;
                    SetPageCacheNoStore();
                    return View(reportStu);
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
            

        }// generate student report

        public ActionResult groupReport(int? id)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    ViewData["userID"] = "" + id;
                    List<Country> countries = db.Countries.ToList();
                    SetPageCacheNoStore();
                    return View(countries);
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

        }// group report

        public ActionResult generateGroupReport(int? loggedId, int? country)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    groupStudentVM groupReport = new groupStudentVM();
                    int totH = 0;
                    List<StudentVM> studentList = new List<StudentVM>();
                    List<Sys_User> userList = db.Sys_User.ToList().FindAll(s => s.User_Role_ID == 1 && s.Country_ID == country);
                    foreach (var s in userList)
                    {
                        StudentVM temp = new StudentVM();
                        temp.title = db.Titles.ToList().Find(t => t.Title_ID == s.Title_ID).TitleDesc;
                        temp.name = s.FName + " " + s.Surname;
                        Student stu = db.Students.ToList().Find(st => st.SysUser_ID == s.SysUser_ID);
                        temp.licence = "" + stu.Licence_No;
                        temp.hoursFlown = "" + stu.Hours_Flown;


                        List<ExamAverageVM> avgs = new List<ExamAverageVM>();

                        List<Student_Exam> sExams = db.Student_Exam.ToList().FindAll(e => e.Student_ID == stu.Student_ID);
                        foreach (var se in sExams)
                        {
                            ExamAverageVM avg = new ExamAverageVM();
                            avg.examId = se.Exam_ID;
                            avg.examName = db.Plane_Type.ToList().Find(p => p.Plane_Type_ID == se.Exam_ID).Type_Description;
                            avg.examAvg = Convert.ToInt32(se.Exam_Mark);
                            avgs.Add(avg);

                        }// for each
                        temp.examAverages = avgs;

                        studentList.Add(temp);
                        totH = totH + Convert.ToInt32(stu.Hours_Flown);
                    }// for each
                    groupReport.students = studentList;
                    groupReport.totHours = totH;
                    ViewData["userID"] = "" + loggedId;
                    ViewData["userName"] = db.Sys_User.ToList().Find(s => s.SysUser_ID == loggedId).FName + " " + db.Sys_User.ToList().Find(s => s.SysUser_ID == loggedId).Surname;
                    SetPageCacheNoStore();
                    return View(groupReport);
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
            
        }// generate group student report

        public ActionResult planeReport(int? id)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    ViewData["userID"] = "" + id;
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    List<Plane> planes = db.Planes.ToList();
                    SetPageCacheNoStore();
                    return View(planes);
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

        }// plane report

        public ActionResult generatePlaneReport(int? loggedId, int? id)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    ViewData["userID"] = "" + loggedId;
                    ViewData["userName"] = db.Sys_User.ToList().Find(s => s.SysUser_ID == loggedId).FName + " " + db.Sys_User.ToList().Find(s => s.SysUser_ID == loggedId).Surname;
                    Plane tempPlane = db.Planes.ToList().Find(p => p.Plane_ID == id);
                    planeVM reportPlane = new planeVM();

                    reportPlane.Plane_Type_ID = tempPlane.Plane_Type_ID;
                    reportPlane.Type_Description = db.Plane_Type.ToList().Find(p => p.Plane_Type_ID == tempPlane.Plane_Type_ID).Type_Description;
                    reportPlane.planeHours = Convert.ToInt32(tempPlane.Hours_Flown);
                    reportPlane.Plane_ID = tempPlane.Plane_ID;
                    reportPlane.Call_Sign = tempPlane.Call_Sign;
                    reportPlane.Description = tempPlane.Description;
                    reportPlane.services = db.Plane_Service.ToList().FindAll(s => s.Plane_ID == tempPlane.Plane_ID);
                    SetPageCacheNoStore();
                    return View(reportPlane);
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
            
        }// generate plane report

        public ActionResult instructorReport(int? id)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    ViewData["userID"] = "" + id;
                    List<Sys_User> instructors = db.Sys_User.ToList().FindAll(s => s.User_Role_ID == 2);
                    SetPageCacheNoStore();
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

        }// instructor report

        public ActionResult generateInstructorReport(int? loggedId, int? id)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    InstructorVM insReport = new InstructorVM();
                    Sys_User tempIns = db.Sys_User.ToList().Find(s => s.SysUser_ID == id);
                    Instructor instructor = db.Instructors.ToList().Find(i => i.SysUser_ID == tempIns.SysUser_ID);
                    insReport.title = db.Titles.ToList().Find(t => t.Title_ID == tempIns.Title_ID).TitleDesc;
                    insReport.name = tempIns.FName + " " + tempIns.Surname;
                    //List<Student_Instructor> tempList = db.Student_Instructor.ToList().FindAll(i => i.Instructor_ID == instructor.Instructor_ID);
                    try
                    {
                        if (db.Student_Instructor.ToList().FindAll(i => i.Instructor_ID == instructor.Instructor_ID) != null)
                        {
                            insReport.students = db.Student_Instructor.ToList().FindAll(i => i.Instructor_ID == instructor.Instructor_ID).Count();
                        }// if statement
                        else
                        {
                            insReport.students = 0;
                        }
                    }
                    catch
                    {
                        insReport.students = 0;
                    }
                    insReport.licence = "" + instructor.Licence_No;
                    int totHours = 0;
                    foreach (var h in db.Student_Instructor.ToList().FindAll(i => i.Instructor_ID == instructor.Instructor_ID))
                    {
                        Student temp = db.Students.ToList().Find(s => s.Student_ID == h.Student_ID);
                        totHours = totHours + Convert.ToInt32(temp.Hours_Flown);
                    }// for each

                    insReport.studentHours = totHours;

                    List<ExamAverageVM> avgs = new List<ExamAverageVM>();
                    foreach (var p in db.Plane_Type.ToList())
                    {
                        try
                        {
                            ExamAverageVM avg = new ExamAverageVM();
                            List<Student_Instructor> stuIns = db.Student_Instructor.ToList().FindAll(i => i.Instructor_ID == instructor.Instructor_ID);
                            Exam tempExam = db.Exams.ToList().Find(e => e.Plane_Type_ID == p.Plane_Type_ID);
                            avg.examId = tempExam.Exam_ID;
                            avg.examName = p.Type_Description;
                            int tot = 0;
                            List<Student_Exam> examList = new List<Student_Exam>();
                            foreach(var t in stuIns)
                            {
                                Student_Exam temp = db.Student_Exam.ToList().Find(s => s.Exam_ID == tempExam.Exam_ID && s.Exam_Mark != 0 && s.Student_ID == t.Student_ID);
                                if(temp != null)
                                {
                                    examList.Add(temp);
                                }// if
                            }// for each
                            foreach (var se in examList)
                            {
                                if (se != null)
                                {
                                    tot = tot + Convert.ToInt32(se.Exam_Mark);
                                    int average = (tot / examList.Count);
                                    avg.examAvg = average;
                                }// inner if statement
                            }// for each
                            avgs.Add(avg);
                        }
                        catch
                        {
                            ExamAverageVM avg = new ExamAverageVM();
                            Exam tempExam = db.Exams.ToList().Find(e => e.Plane_Type_ID == p.Plane_Type_ID);
                            avg.examId = tempExam.Exam_ID;
                            avg.examName = p.Type_Description;
                            avg.examAvg = 0;
                            avgs.Add(avg);
                        }

                    }// for each

                    insReport.examAverages = avgs;

                    ViewData["userID"] = "" + loggedId;
                    ViewData["userName"] = db.Sys_User.ToList().Find(s => s.SysUser_ID == loggedId).FName + " " + db.Sys_User.ToList().Find(s => s.SysUser_ID == loggedId).Surname;
                    SetPageCacheNoStore();
                    return View(insReport);
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
            
        }// generate instructor report

        public ActionResult examDateSelect(int? id)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    ViewData["userID"] = "" + id;
                    SetPageCacheNoStore();
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

        }// date select for exam report

        public ActionResult examReport(int? id, DateTime startDate)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    List<ExamAverageVM> averages = new List<ExamAverageVM>();
                    List<Exam> exams = db.Exams.ToList();
                    foreach (var e in exams)
                    {
                        ExamAverageVM tempAverage = new ExamAverageVM();
                        List<Student_Exam> stuExamList = db.Student_Exam.ToList().FindAll(s => s.Date_Completed >= startDate && s.Exam_ID == e.Exam_ID);
                        int count = stuExamList.Count();
                        int totalMark = 0;
                        tempAverage.examId = e.Exam_ID;
                        tempAverage.examName = db.Plane_Type.ToList().Find(p => p.Plane_Type_ID == e.Plane_Type_ID).Type_Description;
                        if (stuExamList.Count != 0)
                        {
                            foreach (var s in stuExamList)
                            {
                                totalMark = totalMark + Convert.ToInt32(s.Exam_Mark);
                            }// inner for each
                            tempAverage.examAvg = (totalMark / count);
                        }// if statement
                        else
                        {
                            tempAverage.examAvg = 0;
                        }// else statement
                        averages.Add(tempAverage);
                    }// for each

                    AveragesVM avgs = new AveragesVM();
                    avgs.examAverages = averages;
                    ViewData["userID"] = "" + id;
                    ViewData["userName"] = db.Sys_User.ToList().Find(s => s.SysUser_ID == id).FName + " " + db.Sys_User.ToList().Find(s => s.SysUser_ID == id).Surname;
                    ViewBag.DataPoints = JsonConvert.SerializeObject(averages, _jsonSetting);
                    SetPageCacheNoStore();
                    return View(avgs);
                }
                else
                {
                    return RedirectToAction("loginScreen", "Sys_User");
                }
            }
            catch(Exception e)
            {
                return RedirectToAction("loginScreen", "Sys_User");
            }
            
        }// exam report
        JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

        public ActionResult eventsDateSelect(int? id)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    ViewData["userID"] = "" + id;
                    SetPageCacheNoStore();
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

        }// events report date select

        public ActionResult eventsReport(int? id, DateTime startDate)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    List<uEvent> events = db.uEvents.ToList().FindAll(u => u.Start >= startDate);
                    ViewData["userID"] = "" + id;
                    ViewData["userName"] = db.Sys_User.ToList().Find(s => s.SysUser_ID == id).FName + " " + db.Sys_User.ToList().Find(s => s.SysUser_ID == id).Surname;
                    SetPageCacheNoStore();
                    return View(events);
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
            
        }// events report

        public ActionResult Index()
        {
            return View();
        }
    }
}