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
    public class ReportsController : Controller
    {

        private SkyExamsEntities db = new SkyExamsEntities();

        public ActionResult reportsScreen(int? id)
        {
            Sys_User user = db.Sys_User.ToList().Find(s => s.SysUser_ID == id);
            return View(user);
        }// reports screen

        public ActionResult studentReport(int? id)
        {
            ViewData["userID"] = "" + id;
            List<Sys_User> students = db.Sys_User.ToList().FindAll(s => s.User_Role_ID == 1);
            return View(students);
        }// student report

        public ActionResult generateStudentReport(int? loggedId, int? id)
        {
            Sys_User user = db.Sys_User.ToList().Find(s => s.SysUser_ID == id);
            Student stu = db.Students.ToList().Find(s => s.SysUser_ID == user.SysUser_ID);
            StudentVM reportStu = new StudentVM();

            reportStu.title = db.Titles.ToList().Find(t => t.Title_ID == user.Title_ID).TitleDesc;
            reportStu.name = user.FName + " " + user.Surname;
            reportStu.licence = "" + stu.Licence_No;
            reportStu.hoursFlown = "" + stu.Hours_Flown;
            List<Student_Exam> sExams = db.Student_Exam.ToList().FindAll(e => e.Student_ID == stu.Student_ID);
            reportStu.examMarks = sExams;

            List <Plane_Type> temp = db.Plane_Type.ToList();
            List<Plane_Type> types = new List<Plane_Type>();

            foreach(var e in sExams)
            {
                foreach(var p in temp)
                {
                    if(p.Plane_Type_ID == e.Exam_ID)
                    {
                        types.Add(p);
                    }// if statement
                }// inner for each
            }// for each
            reportStu.planeTypes = types;

            ViewData["userID"] = "" + loggedId;
            return View(reportStu);

        }// generate student report

        public ActionResult groupReport(int? id)
        {
            ViewData["userID"] = "" + id;
            List<Country> countries = db.Countries.ToList();
            return View(countries);
        }// student report

        public ActionResult generateGroupReport(int? loggedId, int? country)
        {
            groupStudentVM groupReport = new groupStudentVM();
            int totH = 0;
            List<StudentVM> studentList = new List<StudentVM>();
            List<Sys_User> userList = db.Sys_User.ToList().FindAll(s => s.User_Role_ID == 1 && s.Country_ID == country);
            foreach(var s in userList)
            {
                StudentVM temp = new StudentVM();
                temp.title = db.Titles.ToList().Find(t => t.Title_ID == s.Title_ID).TitleDesc;
                temp.name = s.FName + " " + s.Surname;
                Student stu = db.Students.ToList().Find(st => st.SysUser_ID == s.SysUser_ID);
                temp.licence = "" + stu.Licence_No;
                temp.hoursFlown = "" + stu.Hours_Flown;
                List<Student_Exam> sExams = db.Student_Exam.ToList().FindAll(e => e.Student_ID == stu.Student_ID);
                temp.examMarks = sExams;

                List<Plane_Type> tempPlanes = db.Plane_Type.ToList();
                List<Plane_Type> types = new List<Plane_Type>();

                foreach (var e in sExams)
                {
                    foreach (var p in tempPlanes)
                    {
                        if (p.Plane_Type_ID == e.Exam_ID)
                        {
                            types.Add(p);
                        }// if statement
                    }// inner for each
                }// for each
                temp.planeTypes = types;

                studentList.Add(temp);
                totH = totH + Convert.ToInt32(stu.Hours_Flown);
            }// for each
            groupReport.students = studentList;
            groupReport.totHours = totH;
            ViewData["userID"] = "" + loggedId;
            return View(groupReport);
        }// generate group student report

        public ActionResult planeReport(int? id)
        {
            ViewData["userID"] = "" + id;
            List<Plane> planes = db.Planes.ToList();
            return View(planes);
        }// plane report

        public ActionResult generatePlaneReport(int? loggedId, int? id)
        {
            ViewData["userID"] = "" + loggedId;
            Plane tempPlane = db.Planes.ToList().Find(p => p.Plane_ID == id);
            planeVM reportPlane = new planeVM();

            reportPlane.Plane_Type_ID = tempPlane.Plane_Type_ID;
            reportPlane.Type_Description = db.Plane_Type.ToList().Find(p => p.Plane_Type_ID == tempPlane.Plane_Type_ID).Type_Description;
            reportPlane.Plane_ID = tempPlane.Plane_ID;
            reportPlane.Call_Sign = tempPlane.Call_Sign;
            reportPlane.Description = tempPlane.Description;
            reportPlane.services = db.Plane_Service.ToList().FindAll(s => s.Plane_ID == tempPlane.Plane_ID);
            return View(reportPlane);
        }// generate plane report

        public ActionResult instructorReport(int? id)
        {
            ViewData["userID"] = "" + id;
            List<Sys_User> instructors = db.Sys_User.ToList().FindAll(s => s.User_Role_ID == 2);
            return View(instructors);
        }// instructor report

        public ActionResult generateInstructorReport(int? loggedId, int? id)
        {
            InstructorVM insReport = new InstructorVM();
            Sys_User tempIns = db.Sys_User.ToList().Find(s => s.SysUser_ID == id);
            Instructor instructor = db.Instructors.ToList().Find(i => i.SysUser_ID == tempIns.SysUser_ID);
            insReport.title = db.Titles.ToList().Find(t => t.Title_ID == tempIns.Title_ID).TitleDesc;
            insReport.name = tempIns.FName + " " + tempIns.Surname;
            insReport.students = db.Student_Instructor.ToList().FindAll(i => i.Instructor_ID == instructor.Instructor_ID).Count();
            insReport.licence = "" + instructor.Licence_No;
            int totHours = 0;
            foreach(var h in db.Student_Instructor.ToList().FindAll(i => i.Instructor_ID == instructor.Instructor_ID))
            {
                Student temp = db.Students.ToList().Find(s => s.Student_ID == h.Student_ID);
                totHours = totHours + Convert.ToInt32(temp.Hours_Flown);
            }// for each

            insReport.studentHours = totHours;

            List<ExamAverageVM> avgs = new List<ExamAverageVM>();
            foreach (var p in db.Plane_Type.ToList())
            {
                List<Student_Instructor> stuIns = db.Student_Instructor.ToList().FindAll(i => i.Instructor_ID == instructor.Instructor_ID);
                ExamAverageVM avg = new ExamAverageVM();
                avg.examId = p.Plane_Type_ID;
                avg.examName = p.Type_Description;
                int tot = 0;
                foreach(var s in stuIns)
                {
                    Exam tExam = db.Exams.ToList().Find(t => t.Plane_Type_ID == p.Plane_Type_ID);
                    Student_Exam tempExam = db.Student_Exam.ToList().Find(e => e.Student_ID == s.Student_ID && e.Exam_ID == tExam.Exam_ID);
                    if(tempExam != null)
                    {
                        tot = tot + Convert.ToInt32(tempExam.Exam_Mark);
                        int average = (tot / insReport.students);
                        avg.examAvg = average;
                        avgs.Add(avg);
                    }// inner if statement
                }// inner foreach
                
            }// for each

            insReport.examAverages = avgs;

            ViewData["userID"] = "" + loggedId;
            return View(insReport);
        }// generate instructor report

        public ActionResult Index()
        {
            return View();
        }
    }
}