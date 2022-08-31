﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SkyExams.Models;
using MimeKit;
using MailKit;
using MailKit.Net.Smtp;

namespace SkyExams.Controllers
{
    public class HelpController : Controller
    {

        private SkyExamsEntities db = new SkyExamsEntities();

        public ActionResult studentCheck(int? id)
        {
            Sys_User temp = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
            int role = (int)temp.User_Role_ID;
            if (role == 1)
            {
                return RedirectToAction("studentHelp", new { id = id });
            }
            else
            {
                return RedirectToAction("helpScreen", new { id = id });
            }
        }// student check

        public ActionResult studentHelp(int? id)
        {
            return View(db.Sys_User.ToList().Find(s => s.SysUser_ID == id));
        }// student help

        public ActionResult helpScreen(int? id)
        {
            return View(db.Sys_User.ToList().Find(s => s.SysUser_ID == id));
        }// general help

        public ActionResult viewAccountHelp(int? id)
        {
            Sys_User temp = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
            int role = (int)temp.User_Role_ID;
            if (role == 1)
            {
                ViewData["help"] = "By clicking on the “view account” button, you can view your account details.\nTo update your account, click on the “update account details” button and enter your updated information in the relevant fields.\nTo reset your password, click on the “Reset password” button and enter your new password.\nAs a student, you can update your hours flown by clicking the “update personal hours flown” button and entering your updated hours.";
            }
            else
            {
                ViewData["help"] = "By clicking on the “view account” button, you can view your account details.\nTo update your account, click on the “update account details” button and enter your updated information in the relevant fields." +
                    "To reset your password, click on the “Reset password” button and enter your new password.\nWhen a student registers for a new rating you can capture that registration by clicking on the “Capture registration” button. Select the student you want to capture a registration for and the plane they have registered for. By clicking the download excel document link, you can download an excel spreadsheet with all the registration information." +
                    "You can generate and download different reports by clicking on the “Reports” button.";
            }
            return View(temp);
        }// view account help

        public ActionResult searchHelp(int? id)
        {
            Sys_User temp = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
            int role = (int)temp.User_Role_ID;
            if (role == 1)
            {
                ViewData["help"] = "By clicking on the “search” button you can search for different users under different user profiles.";
            }
            else
            {
                ViewData["help"] = "By clicking on the “search” button you can search for different users under different user profiles.\nOnce you have searched for a user you can delete them from the system or update their user role. If the users you have searched for, is a student, you can assign them to an instructor, add new study resources for them to access, or add them to a new exam.";
            }
            return View(temp);
        }// search help

        public ActionResult planesHelp(int? id)
        {
            Sys_User temp = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
            int role = (int)temp.User_Role_ID;
            if (role == 1)
            {
                ViewData["help"] = "By clicking on the “Planes” button, you will be able to view all the plane types at the flight school. Then by clicking on the “View planes” button, you will be able to view all the planes of that plane type.";
            }
            else
            {
                ViewData["help"] = "By clicking on the “Planes” button, you will be able to view all the plane types at the flight school. Then by clicking on the “View planes” button, you will be able to view all the planes of that plane type.\nBy clicking on the relevant buttons, you will be able to add, update, and delete plane types as well as individual planes. You can also update the individual plane’s hours flown as and book them out for services and capture their service details.";
            }
            return View(temp);
        }// planes help

        public ActionResult lessonPlanHelp(int? id)
        {
            Sys_User temp = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
            int role = (int)temp.User_Role_ID;
            if (role == 1)
            {
                ViewData["help"] = "By clicking on the “Lesson Plan” button, you will be able to see all the plane types. Then by clicking on the relevant “View Lesson Plans” button you will be able to view all that plane’s Lesson Plans. You will be able to view/download individual Lesson Plans by clicking on the view/download link.";
            }
            else
            {
                ViewData["help"] = "By clicking on the “Lesson Plan” button, you will be able to see all the plane types. Then by clicking on the relevant “View Lesson Plans” button you will be able to view all that plane’s Lesson Plans. You will be able to view/download individual Lesson Plans by clicking on the view/download link.\nYou can add or remove study resources by clicking on the relevant buttons.";
            }
            return View(temp);
        }// lesson plan help

        public ActionResult studyResourceHelp(int? id)
        {
            Sys_User temp = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
            int role = (int)temp.User_Role_ID;
            if (role == 1)
            {
                ViewData["help"] = "By clicking on the “Study resources” button, you will be able to see all the plane types. Then by clicking on the relevant “Resources’ button you will be able to view all that plane’s resources. You will be able to view/download individual study resources by clicking on the view/download link.";
            }
            else
            {
                ViewData["help"] = "By clicking on the “Study resources” button, you will be able to see all the plane types. Then by clicking on the relevant “Resources’ button you will be able to view all that plane’s resources. You will be able to view/download individual study resources by clicking on the view/download link.\nYou can add or remove study resources by clicking on the relevant buttons.";
            }
            return View(temp);
        }// study resource help

        public ActionResult examHelp(int? id)
        {
            Sys_User temp = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
            int role = (int)temp.User_Role_ID;
            if (role == 1)
            {
                ViewData["help"] = "By clicking on the “Exams” button you will be able to view all the exams you have access to. By clicking on the “Exams” button under the relevant plane type, you will be able to begin that exam as well as being able to download the load sheet. Once you have begun the exam, you will have to complete all the questions before pressing the “Submit” button. You can also save your exam by pressing the “Save exam” button, so you can complete the questions at another time.";
            }
            else
            {
                ViewData["help"] = "By clicking on the “Exam” button you will be able to see all the plane types. By clicking on an “exam” button under a plane type, you will be able to view all the questions for that plane type. You can create, update, delete, and view the answer to a specific question. You can also upload and delete the load sheet for that exam. You can also view/download the load sheet by clicking the view/download load sheet link.";
            }
            return View(temp);
        }// exam help

        public ActionResult consultationHelp(int? id)
        {
            Sys_User temp = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
            int role = (int)temp.User_Role_ID;
            if (role == 1)
            {
                ViewData["help"] = "By clicking on the “Exam consultations” button, you can view all your booked consultations. You can also book a new consultation as well as update or delete existing consultations by clicking on the relevant buttons.";
            }
            else
            {
                ViewData["help"] = "By clicking on the “exam consultation” button, you can view all your available and booked consultation sessions. You can also create new booking slots as well as update or delete existing consultation bookings.(Please note that only instructors are able to do this.)";
            }
            return View(temp);
        }// consultation help

        // GET: Help
        public ActionResult Index()
        {
            return View();
        }
    }
}