using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Timers;
using SkyExams.Models;
using MimeKit;
using MailKit;
using MailKit.Net.Smtp;
using System.IO;
using ExcelDataReader;
using OfficeOpenXml;
using NJsonSchema.Annotations;
using GroupDocs.Conversion.Options.Convert;
using GroupDocs.Conversion;
using ClosedXML.Excel;
using Timer = SkyExams.Models.Timer;

namespace SkyExams.Controllers
{
    public class Sys_UserController : Controller
    {
        private SkyExamsEntities db = new SkyExamsEntities();
        private masterEntities db2 = new masterEntities();
        private Sys_User user = new Sys_User();

        // GET: Sys_User
        public ActionResult Index()
        {
            return View(db.Sys_User.ToList());
        }

        [HttpGet]
        public ActionResult TimerView(int? id, string err)
        {
            try
            {
                if (id != null)
                {
                    Timer t = db.Timers.ToList().Find(time => time.Timer_ID == 1);
                    ViewData["userId"] = id;
                    ViewData["error"] = err;
                    ViewData["time"] = db.Timers.ToList().Find(ti => ti.Timer_ID == 1).Timer_Value * 60000;
                    return View(t);
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

        }// timer get

        [HttpPost]
        public ActionResult TimerView(int? id, int? time)
        {
            if (time == null || time < 1)
            {
                string temp = "Please complete all the required fields";
                return RedirectToAction("TimerView", new { id = id, err = temp });
            }// if statement
            else
            {
                Timer t = db.Timers.ToList().Find(ti => ti.Timer_ID == 1);
                Timer newTime = new Timer();
                newTime = t;
                newTime.Timer_Value = Convert.ToInt32(time);
                db.Entry(newTime).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("viewAccount", new { id = id });
            }// else

        }// timer post

        public int getTimer()
        {
            return db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value;
        }

        public ActionResult loginScreen()
        {
            return View();
        }// returns Login screen

        [HttpPost]
        public ActionResult loginScreen(string userName, string password)
        {
            string passwordFromDb = "";
            string authId = Guid.NewGuid().ToString();

            Session["AuthID"] = authId;

            var cookie = new HttpCookie("AuthID");
            cookie.Value = authId;
            Response.Cookies.Add(cookie);
            Sys_User loginUser = new Sys_User();
            if (userName != "" || password != "")
            {
                foreach (Sys_User tempUser in db.Sys_User.ToList())
                {
                    if (userName == tempUser.User_Name)
                    {
                        loginUser = tempUser;
                        foreach (UserPassword tempPass in db.UserPasswords.ToList())
                        {
                            if (loginUser.Password_ID == tempPass.Password_ID)
                            {
                                passwordFromDb = decodePassword(tempPass.Encrypted_password);
                                if (password == passwordFromDb)
                                {
                                    return RedirectToAction("homeScreen", new { id = loginUser.SysUser_ID });
                                }// checks is entered password matches db password
                            }// matches user and password ids
                        }// searches passwords
                    }// matches username to db username
                }// searches users
                if (userName == "")
                {
                    ViewData["err"] = "Please enter your username";
                }// if username null
                else
                {
                    ViewData["err"] = "Username or password is incorrect";
                }// else
                return View();
            }
            else
            {
                if (password == "")
                {
                    ViewData["err"] = "Please enter your password";
                }// password null
                else
                {
                    ViewData["err"] = "Please complete all the required fields";
                }//else
                return View();
            }
            // verify username and password here, if correct then display home screen, else login screen with a pop up
        }// returns home screen

        public ActionResult homeScreen(int? id)
        {
            //ViewBag.Title = "Home";
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    Sys_User loggedInUser = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
                    return View(loggedInUser);
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
        }// returns home screen

        public ActionResult registerScreen()
        {
            return View();
        }// return register screen

        [HttpPost]
        public ActionResult registerScreen(string firstName, string lastName, string uName, string title, string cellNo, string email, string pAddress, string country, string city, string zip, string dob, string empStatus, string password, string confPass)
        {
            if (firstName == "" || lastName == "" || uName == "" || title == "" || cellNo == "" || email == "" || pAddress == "" || country == "" || city == "" || zip == "" || dob == "" || empStatus == "" || password == "" || confPass == "")
            {
                ViewData["err"] = "Please complete all the required fields";
                return View();
            }// checks that fileds are not empty
            if (password != confPass)
            {
                ViewData["err"] = "Passwords do not match";
                return View();
            }// checks that passwords match
            else
            {
                System.TimeSpan dobDiff = DateTime.Now.Subtract(Convert.ToDateTime(dob));
                if (dobDiff.TotalDays < 5478)
                {
                    ViewData["err"] = "You are not old enough to be registered on this system";
                    return View();
                }// checks user is old enough
                DateTime DOB = DateTime.Parse(dob);
                user.FName = firstName;
                user.Surname = lastName;
                user.Cell_Number = cellNo;
                user.Email_Address = email;
                user.Physical_Address = pAddress;
                user.DOB = DOB;
                user.User_Name = uName;

                List<Country> cList = db.Countries.ToList();
                int countryIndex = -1;
                countryIndex = cList.FindIndex(c => c.Country_Name == country);
                if (countryIndex == -1)
                {
                    Country newCountry = new Country();
                    //newCountry.Country_ID = cList.Count() + 1;
                    newCountry.Country_Name = country;
                    cList.Add(newCountry);
                    user.Country_ID = newCountry.Country_ID;
                    db.Countries.Add(newCountry);
                    db.SaveChanges();
                }//if country dosent exist
                else
                {
                    user.Country_ID = cList[countryIndex].Country_ID;
                }// if country exists

                List<City> cityList = db.Cities.ToList();
                int cityIndex = -1;
                cityIndex = cityList.FindIndex(ci => ci.City_Name == city);
                if (cityIndex == -1)
                {
                    City newCity = new City();
                    //newCity.City_ID = cityList.Count() + 1;
                    newCity.City_Name = city;
                    cityList.Add(newCity);
                    user.City_ID = newCity.City_ID;
                    db.Cities.Add(newCity);
                    db.SaveChanges();
                }// if city dosent exist
                else
                {
                    user.City_ID = cityList[cityIndex].City_ID;
                }// if city exists

                List<Title> tList = db.Titles.ToList();
                foreach (Title tempTitle in db.Titles.ToList())
                {
                    if (title == tempTitle.TitleDesc)
                    {
                        user.Title_ID = tempTitle.Title_ID;
                    }// if title exists
                }// loops through all titles, if the title exists, is used that ID for the user, if not, it creates a new title and uses that ID

                List<Employment_Status> eList = db.Employment_Status.ToList();
                foreach (Employment_Status tempStatus in db.Employment_Status.ToList())
                {
                    if (empStatus == tempStatus.EmpStatus)
                    {
                        user.Employment_ID = tempStatus.Employment_ID;
                    }// if status exists
                }// loops through all statuses, if the status exists, is used that ID for the user, if not, it creates a new status and uses that ID

                List<Zip_Code> zList = db.Zip_Code.ToList();
                int zipIndex = -1;
                zipIndex = zList.FindIndex(z => z.Code == zip);
                if (zipIndex == -1)
                {
                    Zip_Code newZip = new Zip_Code();
                    //newZip.Zip_ID = zList.Count() + 1;
                    newZip.Code = zip;
                    zList.Add(newZip);
                    user.ZIP_ID = newZip.Zip_ID;
                    db.Zip_Code.Add(newZip);
                    db.SaveChanges();
                }// if zip dosent exist
                else
                {
                    user.ZIP_ID = zList[zipIndex].Zip_ID;
                }// if zip exists

                List<UserPassword> passList = db.UserPasswords.ToList();
                UserPassword newPassword = new UserPassword();
                //newPassword.Password_ID = passList.Count + 1;
                string encPassword = encodePassword(confPass);
                newPassword.Encrypted_password = encPassword;
                newPassword.Date_Set = DateTime.Now;
                passList.Add(newPassword);
                db.UserPasswords.Add(newPassword);
                db.SaveChanges();
                user.Password_ID = newPassword.Password_ID;
                //user.SysUser_ID = db.Sys_User.ToList().Count + 1;
                db.Sys_User.Add(user);
                db.SaveChanges();

                try
                {
                    //create email
                    MimeMessage requestEmail = new MimeMessage();
                    requestEmail.From.Add(new MailboxAddress("New user", "skyexams.fts@gmail.com"));
                    requestEmail.To.Add(MailboxAddress.Parse("skyexams.fts@gmail.com"));
                    requestEmail.Subject = "New user request";
                    requestEmail.Body = new TextPart("plain") { Text = "A new user wishes to be registered on the system: User name: " + firstName + " " + lastName + " email address: " + email };

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
                    ViewData["err"] = "Email failed to send.";
                    return View();
                }
                return RedirectToAction("registrationConformationScreen");
            }// adds user, emails admin

        }// registering users

        public ContentResult SaveCapture(string data)
        {
            string fileName = DateTime.Now.ToString("dd-MM-yy hh-mm-ss");

            int index = data.IndexOf("*");
            string image = data.Substring(0, index);
            string user = data.Substring(index);
            string userName = user.Trim('*');

            //Convert Base64 Encoded string to Byte Array.
            byte[] imageBytes = Convert.FromBase64String(image.Split(',')[1]);

            Profile_Image newImg = new Profile_Image();
            newImg.Profile_Image1 = imageBytes;
            newImg.Sys_User_ID = db.Sys_User.ToList().Find(u => u.User_Name == userName).SysUser_ID;

            db.Profile_Image.Add(newImg);
            db.SaveChanges();

            //Save the Byte Array as Image File.
            string filePath = Server.MapPath(string.Format("~/Captures/{0}.jpg", fileName));
            //System.IO.File.WriteAllBytes(filePath, imageBytes);

            return Content("true");
        }// save profile image

        [HttpGet]
        public ActionResult registrationConformationScreen()
        {
            return View();
        }// confirm registration

        [HttpPost]
        public ActionResult registrationConformationScreen(string uName, string code)
        {
            if (uName == "" || code == "")
            {
                ViewData["err"] = "Please complete all the required fields";
                return View();
            }// no input 
            if (code == "101")
            {
                string studentLicence = Request["licence"];
                Student newStudent = new Student();
                //int studentID = db.Students.ToList().Count + 1;
                //newStudent.Student_ID = studentID;
                newStudent.Licence_No = Convert.ToInt32(studentLicence);

                Sys_User newStudentUser = db.Sys_User.ToList().Find(u => u.User_Name == uName);
                newStudentUser.User_Role_ID = 1;
                db.Entry(newStudentUser).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                newStudent.SysUser_ID = newStudentUser.SysUser_ID;
                newStudent.Hours_Flown = 0;
                db.Students.Add(newStudent);
                db.SaveChanges();
                return RedirectToAction("loginScreen");
            }// student 
            if (code == "102")
            {
                string instructorLicence = Request["licence"];
                Instructor newInstructor = new Instructor();
                //int indtructorId = db.Instructors.ToList().Count + 1;
                //newInstructor.Instructor_ID = indtructorId;
                newInstructor.Licence_No = Convert.ToInt32(instructorLicence);

                Sys_User newInsUser = db.Sys_User.ToList().Find(u => u.User_Name == uName);
                newInsUser.User_Role_ID = 2;
                db.Entry(newInsUser).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                newInstructor.SysUser_ID = newInsUser.SysUser_ID;
                db.Instructors.Add(newInstructor);
                db.SaveChanges();
                return RedirectToAction("loginScreen");
            }// instructor
            if (code == "103")
            {
                Admin newAdmin = new Admin();
                //int adminId = db.Admins.ToList().Count + 1;
                //newAdmin.Admin_ID = adminId;


                Sys_User newAdminUser = db.Sys_User.ToList().Find(u => u.User_Name == uName);
                newAdminUser.User_Role_ID = 2;
                db.Entry(newAdminUser).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                newAdmin.SysUser_ID = newAdminUser.SysUser_ID;
                db.Admins.Add(newAdmin);
                db.SaveChanges();
                return RedirectToAction("loginScreen");
            }// admin
            if (code == "104")
            {
                Manager newManager = new Manager();
                //int managerId = db.Managers.ToList().Count + 1;
                //newManager.Manager_ID = managerId;

                Sys_User newManagerUser = db.Sys_User.ToList().Find(u => u.User_Name == uName);
                newManagerUser.User_Role_ID = 2;
                db.Entry(newManagerUser).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                newManager.SysUser_ID = newManagerUser.SysUser_ID;
                db.Managers.Add(newManager);
                db.SaveChanges();
                return RedirectToAction("loginScreen");
            }// manager 
            else
            {
                return View();
            }
        }// finalise registration with codes

        public ActionResult viewAccount(int? id, string msg)
        {
            try
            {
                if (id != null)
                {
                    Sys_User viewUser = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
                    ViewData["msg"] = msg;
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    return View(viewUser);
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

        }// view account function

        public ActionResult BtnBackup_Click(int loggedId)
        {
            int obj = db2.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, "skyexamsBackup");

            if (obj != 0)
            {
                string temp = "Backup successfully created.";
                return RedirectToAction("viewAccount", new { id = loggedId, msg = temp });
            }
            else
            {
                return RedirectToAction("loginScreen");
            }
        }//Backup database function

        public ActionResult BtnRestore_Click(int loggedId)
        {

            var obj = db2.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, "skyExamsRestore");

            if (obj != 0)
            {
                string temp = "Restore successfully executed.";
                return RedirectToAction("viewAccount", new { id = loggedId, msg = temp });
            }
            else
            {
                return RedirectToAction("loginScreen");
            }

        }//Restore database function

        public FileContentResult getImg(int id)
        {
            try
            {
                byte[] byteArray = null;
                byteArray = db.Profile_Image.Find(id).Profile_Image1;
                return byteArray != null
                    ? new FileContentResult(byteArray, "image/jpeg")
                    : null;
            }// try
            catch (Exception e)
            {
                return null;
            }// catch
        }// get profile image

        public ActionResult searchScreen(int? id, string err)
        {
            try
            {
                if (id != null)
                {
                    ViewData["err"] = err;
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    Sys_User viewUser = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
                    return View(viewUser);
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
        }// search screen

        public ActionResult searchResultsScreen(int? id, string firstName, string lastName, string list)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    ViewData["userID"] = "" + id;
                    ViewData["role"] = list;
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    Sys_User forRole = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
                    ViewData["userRole"] = "" + forRole.User_Role_ID;
                    List<Sys_User> sUsers = new List<Sys_User>();
                    if (firstName == "" && lastName == "")
                    {
                        string temp = "Hint: Complete all the fields before clicking submit.";
                        return RedirectToAction("searchScreen", new { id = id, err = temp });
                    }// if fields are empty
                    else
                    {
                        if (list == "student")
                        {
                            List<Sys_User> tempStudentList = db.Sys_User.ToList().FindAll(s => s.User_Role_ID == 1);
                            sUsers = tempStudentList.FindAll(u => u.FName == firstName || u.Surname == lastName);
                        }// students
                        if (list == "instructor")
                        {
                            List<Sys_User> tempInstructorList = db.Sys_User.ToList().FindAll(s => s.User_Role_ID == 2);
                            sUsers = tempInstructorList.FindAll(u => u.FName == firstName || u.Surname == lastName);
                        }// instructors
                        if (list == "admin")
                        {
                            List<Sys_User> tempAdminList = db.Sys_User.ToList().FindAll(s => s.User_Role_ID == 3);
                            sUsers = tempAdminList.FindAll(u => u.FName == firstName || u.Surname == lastName);
                        }// admin
                        if (list == "manager")
                        {
                            List<Sys_User> tempManagerList = db.Sys_User.ToList().FindAll(s => s.User_Role_ID == 4);
                            sUsers = tempManagerList.FindAll(u => u.FName == firstName || u.Surname == lastName);
                        }// manager
                    }// fields arent empty
                    return View(sUsers);
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
        }// displays search results

        public ActionResult updateAccount(int? id, string err)
        {
            try
            {
                if (id != null)
                {
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    ViewData["err"] = err;
                    Sys_User updateUser = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
                    return View(updateUser);
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

        }// return update screen

        [HttpPost]
        public ActionResult updateAccount(string id, string firstName, string lastName, string uName, string title, string cellNo, string email, string pAddress, string country, string city, string zip, string dob, string empStatus)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    int idUser = Convert.ToInt32(id);
                    Sys_User sys_User = db.Sys_User.ToList().Find(u => u.SysUser_ID == idUser);
                    if (firstName == "" || lastName == "" || uName == "" || title == "" || cellNo == "" || email == "" || pAddress == "" || country == "" || city == "" || zip == "" || dob == "" || empStatus == "")
                    {
                        string temp = "Hint: Complete all the fields before clicking submit.";
                        return RedirectToAction("updateAccount", new { id = idUser, err = temp });
                    }// checks that fileds are not empty
                    else
                    {
                        Sys_User updateUser = new Sys_User();
                        DateTime DOB = DateTime.Parse(dob);
                        updateUser = sys_User;
                        updateUser.FName = firstName;
                        updateUser.Surname = lastName;
                        updateUser.Cell_Number = cellNo;
                        updateUser.Email_Address = email;
                        updateUser.Physical_Address = pAddress;
                        updateUser.DOB = DOB;
                        updateUser.User_Name = uName;

                        List<Country> cList = db.Countries.ToList();
                        int countryIndex = -1;
                        countryIndex = cList.FindIndex(c => c.Country_Name == country);
                        if (countryIndex == -1)
                        {
                            Country newCountry = new Country();
                            //newCountry.Country_ID = cList.Count() + 1;
                            newCountry.Country_Name = country;
                            cList.Add(newCountry);
                            updateUser.Country_ID = newCountry.Country_ID;
                            db.Countries.Add(newCountry);
                            db.SaveChanges();
                        }//if country dosent exist
                        else
                        {
                            updateUser.Country_ID = cList[countryIndex].Country_ID;
                        }// if country exists

                        List<City> cityList = db.Cities.ToList();
                        int cityIndex = -1;
                        cityIndex = cityList.FindIndex(ci => ci.City_Name == city);
                        if (cityIndex == -1)
                        {
                            City newCity = new City();
                            //newCity.City_ID = cityList.Count() + 1;
                            newCity.City_Name = city;
                            cityList.Add(newCity);
                            updateUser.City_ID = newCity.City_ID;
                            db.Cities.Add(newCity);
                            db.SaveChanges();
                        }// if city dosent exist
                        else
                        {
                            updateUser.City_ID = cityList[cityIndex].City_ID;
                        }// if city exists

                        List<Title> tList = db.Titles.ToList();
                        foreach (Title tempTitle in db.Titles.ToList())
                        {
                            if (title == tempTitle.TitleDesc)
                            {
                                updateUser.Title_ID = tempTitle.Title_ID;
                            }// if title exists
                        }// loops through all titles, if the title exists, is used that ID for the user, if not, it creates a new title and uses that ID

                        List<Employment_Status> eList = db.Employment_Status.ToList();
                        foreach (Employment_Status tempStatus in db.Employment_Status.ToList())
                        {
                            if (empStatus == tempStatus.EmpStatus)
                            {
                                updateUser.Employment_ID = tempStatus.Employment_ID;
                            }// if status exists
                        }// loops through all statuses, if the status exists, is used that ID for the user, if not, it creates a new status and uses that ID

                        List<Zip_Code> zList = db.Zip_Code.ToList();
                        int zipIndex = -1;
                        zipIndex = zList.FindIndex(z => z.Code == zip);
                        if (zipIndex == -1)
                        {
                            Zip_Code newZip = new Zip_Code();
                            //newZip.Zip_ID = zList.Count() + 1;
                            newZip.Code = zip;
                            zList.Add(newZip);
                            updateUser.ZIP_ID = newZip.Zip_ID;
                            db.Zip_Code.Add(newZip);
                            db.SaveChanges();
                        }// if zip dosent exist
                        else
                        {
                            updateUser.ZIP_ID = zList[zipIndex].Zip_ID;
                        }// if zip exists

                        db.Entry(updateUser).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        return RedirectToAction("viewAccount", new { id = idUser });
                    }// adds user, emails admin
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


        }// updating users

        public ActionResult addStudentResource(int? loggedId, int? id)
        {
            try
            {
                if (loggedId != null || id != null)
                {
                    ViewData["userID"] = "" + loggedId;
                    ViewData["studentID"] = "" + id;
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    return View(db.Study_Resource.ToList());
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
        }// add student resource get

        [HttpPost]
        public ActionResult addStudentResource(int? loggedId, int? id, string resource)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    try
                    {
                        int resourceId = Convert.ToInt32(resource);
                        Student_Resource newStudentResource = new Student_Resource();
                        int sID = db.Students.ToList().Find(s => s.SysUser_ID == id).Student_ID;
                        newStudentResource.Student_ID = sID;
                        newStudentResource.Study_Resource_ID = resourceId;
                        db.Student_Resource.Add(newStudentResource);
                        db.SaveChanges();
                        Sys_User temp = db.Sys_User.ToList().Find(s => s.SysUser_ID == id);
                        return RedirectToAction("searchResultsScreen", new { id = loggedId, firstName = temp.FName, surname = temp.Surname, list = "student" });
                    }// try
                    catch (Exception ex)
                    {
                        return RedirectToAction("searchScreen", new { id = loggedId });
                    }// catch
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

        }// add student resource post

        [HttpGet]
        public ActionResult addStudentExam(int? loggedId, int? id)
        {
            try
            {
                if (loggedId != null || id != null)
                {
                    ViewData["userID"] = "" + loggedId;
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    ViewData["studentID"] = "" + id;
                    return View(db.Plane_Type.ToList());
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
        }// add student exam get

        [HttpPost]
        public ActionResult addStudentExam(int? loggedId, int? id, int exam)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    try
                    {
                        Student_Exam newStudentExam = new Student_Exam();
                        int stuId = db.Students.ToList().Find(s => s.SysUser_ID == id).Student_ID;
                        newStudentExam.Student_ID = stuId;
                        newStudentExam.Exam_ID = exam;
                        newStudentExam.Exam_Mark = 0;
                        newStudentExam.Started = false;
                        newStudentExam.Completed = false;
                        db.Student_Exam.Add(newStudentExam);
                        db.SaveChanges();
                        Sys_User temp = db.Sys_User.ToList().Find(s => s.SysUser_ID == id);
                        return RedirectToAction("searchResultsScreen", new { id = loggedId, firstName = temp.FName, surname = temp.Surname, list = "student" });
                    }// try
                    catch (Exception ex)
                    {
                        return RedirectToAction("searchScreen", new { id = loggedId });
                    }// catch
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
        }// add student exam post

        public ActionResult addStudentInstructor(int? loggedId, int? id)
        {
            try
            {
                if (loggedId != null || id != null)
                {
                    ViewData["userID"] = "" + loggedId;
                    ViewData["studentID"] = "" + id;
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    List<Sys_User> instructors = db.Sys_User.ToList().FindAll(i => i.User_Role_ID == 2);
                    return View(instructors);
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

        }// add student instructor get

        [HttpPost]
        public ActionResult addStudentInstructor(int? loggedId, int? id, string insturctor)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    try
                    {
                        int instructorId = Convert.ToInt32(insturctor);
                        Sys_User ins = db.Sys_User.Find(instructorId);
                        int sID = Convert.ToInt32(id);
                        Sys_User stu = db.Sys_User.Find(sID);
                        Student studentForId = db.Students.ToList().Find(s => s.SysUser_ID == stu.SysUser_ID);
                        Instructor instructorForId = db.Instructors.ToList().Find(i => i.SysUser_ID == ins.SysUser_ID);
                        Student_Instructor newStudentInstructor = new Student_Instructor();
                        newStudentInstructor.Student_ID = studentForId.Student_ID;
                        newStudentInstructor.Instructor_ID = instructorForId.Instructor_ID;
                        db.Student_Instructor.Add(newStudentInstructor);
                        db.SaveChanges();
                        List<Lesson_Plan> planList = db.Lesson_Plan.ToList().FindAll(p => p.Instructor_ID == instructorForId.Instructor_ID);
                        foreach (Lesson_Plan temp in planList)
                        {
                            Student_Lesson_Plan sLessonPlan = new Student_Lesson_Plan();
                            sLessonPlan.Student_ID = studentForId.Student_ID;
                            sLessonPlan.Lesson_Plan_ID = temp.Lesson_Plan_ID;
                            db.Student_Lesson_Plan.Add(sLessonPlan);
                            db.SaveChangesAsync();
                        }// for each
                        Sys_User tempUser = db.Sys_User.ToList().Find(s => s.SysUser_ID == id);
                        return RedirectToAction("searchResultsScreen", new { id = loggedId, firstName = tempUser.FName, surname = tempUser.Surname, list = "student" });
                    }// try
                    catch (Exception ex)
                    {
                        return RedirectToAction("searchScreen", new { id = loggedId });
                    }// catch
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

        }// add student instructor post

        public ActionResult resetPassword(int? id, string err)
        {
            try
            {
                if (id != null)
                {
                    ViewData["err"] = err;
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    Sys_User passwordUser = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
                    return View(passwordUser);
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

        }// reset password

        [HttpPost]
        public ActionResult resetPassword(int? id, string oldPass, string newPass, string confPass)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    if (oldPass == "" || newPass == "" || confPass == "")
                    {
                        string temp = "Hint: Complete all the fields before clicking submit.";
                        return RedirectToAction("resetPassword", new { id = id, err = temp });
                    }// if fields are empty
                    if (newPass != confPass)
                    {
                        return View(db.Sys_User.ToList().Find(u => u.SysUser_ID == id));
                    }// if passwords dont match
                    else
                    {
                        Sys_User tempUser = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
                        int passID = tempUser.Password_ID;
                        UserPassword pass = db.UserPasswords.ToList().Find(p => p.Password_ID == passID);
                        string encPass = pass.Encrypted_password;
                        string decPass = decodePassword(encPass);
                        if (decPass == oldPass)
                        {
                            string encNewPass = encodePassword(confPass);
                            pass.Encrypted_password = encNewPass;
                            pass.Date_Set = DateTime.Now;
                            db.Entry(pass).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChangesAsync();
                        }// if entered password matches db password
                        return RedirectToAction("loginScreen");
                    }// else
                }// if statement
                else
                {
                    return RedirectToAction("loginScreen");
                }
            }
            catch
            {
                return RedirectToAction("loginScreen");
            }

        }// reset password

        public ActionResult forgotPassword()
        {
            return View();
        }// forgot password

        [HttpPost]
        public ActionResult forgotPassword(string userName, string email, string newPass, string confPass)
        {
            if (userName == "" || email == "" || newPass == "" || confPass == "")
            {
                ViewData["err"] = "Please complete all the required fields";
                return View();
            }// if fields are empty
            if (newPass != confPass)
            {
                return View();
            }// if passwords dont match
            else
            {
                Sys_User tempUser = db.Sys_User.ToList().Find(u => u.User_Name == userName && u.Email_Address == email);
                int passID = tempUser.Password_ID;
                UserPassword pass = db.UserPasswords.ToList().Find(p => p.Password_ID == passID);
                string encPass = pass.Encrypted_password;
                string decPass = decodePassword(encPass);
                string encNewPass = encodePassword(confPass);
                pass.Encrypted_password = encNewPass;
                pass.Date_Set = DateTime.Now;
                db.Entry(pass).State = System.Data.Entity.EntityState.Modified;
                db.SaveChangesAsync();
                return RedirectToAction("loginScreen");
            }
        }// forgot password

        public ActionResult deleteUser(int? loggedId, int? id)
        {
            try
            {
                if (loggedId != null || id != null)
                {
                    ViewData["loggedId"] = "" + loggedId;
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    Sys_User delUser = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
                    ViewData["role"] = db.User_Role.ToList().Find(u => u.User_Role_ID == delUser.User_Role_ID).RoleDesc.ToLower();
                    return View(delUser);
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

        }// delete get

        public ActionResult deleteConformation(int? loggedId, int? id)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    Sys_User sys_User = db.Sys_User.Find(id);
                    int passId = sys_User.Password_ID;
                    db.Sys_User.Remove(sys_User);
                    db.SaveChanges();
                    List<UserPassword> passList = db.UserPasswords.ToList();
                    UserPassword delPass = passList.Find(u => u.Password_ID == passId);
                    db.UserPasswords.Remove(delPass);
                    db.SaveChanges();
                    if (sys_User.User_Role_ID == 1)
                    {
                        List<Student> tempStudentList = db.Students.ToList();
                        Student delStudent = tempStudentList.Find(u => u.SysUser_ID == id);
                        db.Students.Remove(delStudent);
                        db.SaveChanges();
                        Student_Instructor delStudentInstructor = db.Student_Instructor.ToList().Find(s => s.Student_ID == delStudent.Student_ID);
                        if (delStudentInstructor != null)
                        {
                            db.Student_Instructor.Remove(delStudentInstructor);
                            db.SaveChanges();
                        }// if not null
                        List<Student_Resource> delStuResource = db.Student_Resource.ToList().FindAll(s => s.Student_ID == id);
                        if (delStuResource != null)
                        {
                            foreach (var r in delStuResource)
                            {
                                db.Student_Resource.Remove(r);
                                db.SaveChanges();
                            }// for each
                        }// if not null
                        List<Student_Lesson_Plan> delStuPlan = db.Student_Lesson_Plan.ToList().FindAll(s => s.Student_ID == id);
                        if (delStuPlan != null)
                        {
                            foreach (var l in delStuPlan)
                            {
                                db.Student_Lesson_Plan.Remove(l);
                                db.SaveChanges();
                            }// for each
                        }// if not null
                    }// students
                    if (sys_User.User_Role_ID == 2)
                    {
                        List<Instructor> tempInstructorList = db.Instructors.ToList();
                        Instructor delInstructor = tempInstructorList.Find(u => u.SysUser_ID == id);
                        db.Instructors.Remove(delInstructor);
                        db.SaveChanges();
                        List<Student_Instructor> delStudentInstructor = db.Student_Instructor.ToList().FindAll(s => s.Instructor_ID == delInstructor.Instructor_ID);
                        if (delStudentInstructor != null)
                        {
                            foreach (var sI in delStudentInstructor)
                            {
                                db.Student_Instructor.Remove(sI);
                                db.SaveChanges();
                            }// for each
                        }// if statement

                        List<Lesson_Plan> planList = db.Lesson_Plan.ToList().FindAll(i => i.Instructor_ID == id);
                        if (planList != null)
                        {
                            foreach (Lesson_Plan temp in planList)
                            {
                                db.Lesson_Plan.Remove(temp);
                                db.SaveChanges();
                            }// del lesson plans
                        }// if statement
                    }// instructors
                    if (sys_User.User_Role_ID == 3)
                    {
                        List<Admin> tempAdminList = db.Admins.ToList();
                        Admin delAdmin = tempAdminList.Find(u => u.SysUser_ID == id);
                        db.Admins.Remove(delAdmin);
                        db.SaveChanges();
                    }// admin
                    if (sys_User.User_Role_ID == 4)
                    {
                        List<Manager> tempManagerList = db.Managers.ToList();
                        Manager delManager = tempManagerList.Find(u => u.SysUser_ID == id);
                        db.Managers.Remove(delManager);
                        db.SaveChanges();
                    }// manager
                    return RedirectToAction("searchScreen", new { id = loggedId });
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

        }

        [HttpGet]
        public ActionResult UpdateUserRole(int? loggedId, int? id)
        {
            try
            {
                if (loggedId != null || id != null)
                {
                    ViewData["loggedId"] = "" + loggedId;
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    Sys_User updateRole = db.Sys_User.ToList().Find(s => s.SysUser_ID == id);
                    ViewData["role"] = db.User_Role.ToList().Find(u => u.User_Role_ID == updateRole.User_Role_ID).RoleDesc.ToLower();
                    return View(updateRole);
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

        }//update user role get

        [HttpPost]
        public ActionResult UpdateUserRole(int? loggedId, int? id, int role)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    int roleID = Convert.ToInt32(id);
                    Sys_User sys_User = db.Sys_User.ToList().Find(u => u.SysUser_ID == roleID);
                    int uRole = Convert.ToInt32(sys_User.User_Role_ID);
                    Sys_User updateRole = new Sys_User();
                    updateRole = sys_User;
                    updateRole.User_Role_ID = role;
                    List<User_Role> roleList = db.User_Role.ToList();

                    if (uRole == 1)
                    {
                        Student tempStudent = db.Students.ToList().Find(u => u.SysUser_ID == sys_User.SysUser_ID);
                        db.Students.Remove(tempStudent);
                        db.SaveChanges();
                    }
                    if (uRole == 2)
                    {
                        Instructor tempStudent = db.Instructors.ToList().Find(u => u.SysUser_ID == sys_User.SysUser_ID);
                        db.Instructors.Remove(tempStudent);
                        db.SaveChanges();
                    }
                    if (uRole == 3)
                    {
                        Admin tempStudent = db.Admins.ToList().Find(u => u.SysUser_ID == sys_User.SysUser_ID);
                        db.Admins.Remove(tempStudent);
                        db.SaveChanges();
                    }
                    if (uRole == 4)
                    {
                        Manager tempStudent = db.Managers.ToList().Find(u => u.SysUser_ID == sys_User.SysUser_ID);
                        db.Managers.Remove(tempStudent);
                        db.SaveChanges();
                    }

                    if (role == 1)
                    {
                        string studentLicence = Request["licence"];
                        Student newStudent = new Student();
                        newStudent.SysUser_ID = updateRole.SysUser_ID;
                        int studentID = db.Students.ToList().Count + 2;
                        newStudent.Student_ID = studentID;
                        newStudent.Licence_No = Convert.ToInt32(studentLicence);
                        db.Students.Add(newStudent);
                        db.SaveChanges();
                    }
                    if (role == 2)
                    {
                        string instructorLicence = Request["licence"];
                        Instructor newInstructor = new Instructor();
                        newInstructor.SysUser_ID = updateRole.SysUser_ID;
                        int instructorID = db.Instructors.ToList().Count + 2;
                        newInstructor.Instructor_ID = instructorID;
                        newInstructor.Licence_No = Convert.ToInt32(instructorLicence);
                        db.Instructors.Add(newInstructor);
                        db.SaveChanges();
                    }
                    if (role == 3)
                    {
                        Admin newAdmin = new Admin();
                        newAdmin.SysUser_ID = updateRole.SysUser_ID;
                        int adminID = db.Admins.ToList().Count + 2;
                        newAdmin.Admin_ID = adminID;
                        db.Admins.Add(newAdmin);
                        db.SaveChanges();
                    }
                    if (role == 4)
                    {
                        Manager newManager = new Manager();
                        newManager.SysUser_ID = updateRole.SysUser_ID;
                        int managerID = db.Managers.ToList().Count + 2;
                        newManager.Manager_ID = managerID;
                        db.Managers.Add(newManager);
                        db.SaveChanges();
                    }

                    //code
                    return RedirectToAction("searchScreen", new { id = loggedId });
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

        }//update user role post

        [HttpGet]
        public ActionResult StudentHours(int? id, string err)
        {
            try
            {
                if (id != null)
                {
                    Sys_User stuHours = db.Sys_User.ToList().Find(s => s.SysUser_ID == id);
                    ViewData["userId"] = "" + id;
                    ViewData["userName"] = stuHours.FName + " " + stuHours.Surname;
                    ViewData["err"] = err;
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    Student stu = db.Students.ToList().Find(s => s.SysUser_ID == stuHours.SysUser_ID);
                    return View(stu);
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

        }//Update student hours Get

        [HttpPost]
        public ActionResult StudentHours(int? userId, int? id, int? hoursFlown)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    Student stu = db.Students.ToList().Find(s => s.SysUser_ID == id);
                    Student updateHours = stu;
                    if (hoursFlown != null)
                    {
                        updateHours.Hours_Flown = hoursFlown;
                        db.Entry(updateHours).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        string temp = "Hint: Complete all the fields before clicking submit.";
                        return RedirectToAction("StudentHours", new { id = id, err = temp });
                    }

                    return RedirectToAction("viewAccount", new { id = userId });
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

        }// update student hours post

        [HttpGet]
        public ActionResult captureRegistration(int? id)
        {
            try
            {
                if (id != null)
                {
                    ViewData["userId"] = "" + id;
                    return View(db.Sys_User.ToList().FindAll(s => s.User_Role_ID == 1));
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

        }// capture registration get

        [HttpGet]
        public ActionResult CapturePlaneRegistration(int? loggedId, int? id)
        {
            try
            {
                if (loggedId != null || id != null)
                {
                    ViewData["userId"] = "" + loggedId;
                    ViewData["studentId"] = "" + id;
                    return View(db.Plane_Type.ToList());
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

        }//Capture plane registration get

        [HttpPost]
        public ActionResult CapturePlaneRegistration(int? loggedId, int? id, int plane)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    Registration_Sheet regSheet = new Registration_Sheet();

                    regSheet.Sys_User_ID = Convert.ToInt32(id);
                    regSheet.First_Name = db.Sys_User.ToList().Find(s => s.SysUser_ID == id).FName;
                    regSheet.Surname = db.Sys_User.ToList().Find(s => s.SysUser_ID == id).Surname;
                    regSheet.Plane_Type_ID = plane;
                    regSheet.Type_Desctription = db.Plane_Type.ToList().Find(s => s.Plane_Type_ID == plane).Type_Description;
                    //regSheet.Paid = true;
                    db.Registration_Sheet.Add(regSheet);
                    db.SaveChanges();

                    return RedirectToAction("viewAccount", new { id = loggedId });
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

        }//capture plane registration post

        public FileResult ExportToExcel()
        {
            DataTable dt = new DataTable("Grid");
            dt.Columns.AddRange(new DataColumn[6] { new DataColumn("First Name"),
                                                     new DataColumn("Surname"),
                                                     new DataColumn("Licence Number"),
                                                     new DataColumn("Date Written"),
                                                     new DataColumn("Exam"),
                                                     new DataColumn("Exam Mark")});
            var regSheet = from Registration_Sheet in db.Registration_Sheet select Registration_Sheet;
            foreach (var sheet in regSheet)
            {
                dt.Rows.Add(sheet.First_Name, sheet.Surname, sheet.Licence_No, sheet.Date_Written, sheet.Type_Desctription, sheet.Mark);
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream()) //using System.IO;
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "StudentRegistration.xlsx");
                }
            }
        }//export to excel post

        public static string encodePassword(string password)
        {
            try
            {
                int len = password.Length;
                byte[] encPass_byte = new byte[len];
                encPass_byte = System.Text.Encoding.UTF8.GetBytes(password);
                string encodedPassword = Convert.ToBase64String(encPass_byte);
                Console.Write(encodedPassword);
                return encodedPassword;
            }// try
            catch (Exception ex)
            {
                throw new Exception("Error in base64Encode" + ex);
            }// catch
        }// method to encode the password

        public string decodePassword(string encodedPassword)
        {
            System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
            System.Text.Decoder utf8Decode = encoder.GetDecoder();
            byte[] todecode_byte = Convert.FromBase64String(encodedPassword);
            int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
            char[] decoded_char = new char[charCount];
            utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
            string decodedPassword = new String(decoded_char);
            return decodedPassword;
        }// method to decode the password

        // GET: Sys_User/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sys_User sys_User = db.Sys_User.Find(id);
            if (sys_User == null)
            {
                return HttpNotFound();
            }
            return View(sys_User);
        }

        // GET: Sys_User/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Sys_User/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "SysUser_ID,User_Role_ID,Title_ID,FName,Surname,Cell_Number,Email_Address,Physical_Address,DOB,Password_ID,City_ID,Country_ID,ZIP_ID,Employment_ID")] Sys_User sys_User)
        {
            if (ModelState.IsValid)
            {
                db.Sys_User.Add(sys_User);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(sys_User);
        }

        // GET: Sys_User/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sys_User sys_User = db.Sys_User.Find(id);
            if (sys_User == null)
            {
                return HttpNotFound();
            }
            return View(sys_User);
        }

        // POST: Sys_User/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "SysUser_ID,User_Role_ID,Title_ID,FName,Surname,Cell_Number,Email_Address,Physical_Address,DOB,Password_ID,City_ID,Country_ID,ZIP_ID,Employment_ID")] Sys_User sys_User)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sys_User).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(sys_User);
        }

        // GET: Sys_User/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sys_User sys_User = db.Sys_User.Find(id);
            if (sys_User == null)
            {
                return HttpNotFound();
            }
            return View(sys_User);
        }

        // POST: Sys_User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Sys_User sys_User = db.Sys_User.Find(id);
            db.Sys_User.Remove(sys_User);
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