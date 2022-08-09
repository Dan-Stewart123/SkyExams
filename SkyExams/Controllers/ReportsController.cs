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

        public ActionResult Index()
        {
            return View();
        }
    }
}