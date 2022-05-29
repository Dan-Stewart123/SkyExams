using System;
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
    public class Sys_UserController : Controller
    {
        private SkyExamsEntities db = new SkyExamsEntities();
        private Sys_User user = new Sys_User();

        // GET: Sys_User
        public ActionResult Index()
        {
            return View(db.Sys_User.ToList());
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
                    if(userName == tempUser.User_Name)
                    {
                        loginUser = tempUser;
                        foreach(UserPassword tempPass in db.UserPasswords.ToList())
                        {
                            if(loginUser.Password_ID == tempPass.Password_ID)
                            {
                                passwordFromDb = decodePassword(tempPass.Encrypted_password);
                                if(password == passwordFromDb)
                                {
                                    return RedirectToAction ("homeScreen", new { id = loginUser.SysUser_ID});
                                }// checks is entered password matches db password
                            }// matches user and password ids
                        }// searches passwords
                    }// matches username to db username
                }// searches users
                return View();
            }
            else
            {
                return View();
            }
            // verify username and password here, if correct then display home screen, else login screen with a pop up
        }// returns home screen

        public ActionResult homeScreen(int? id)
        {
            ViewBag.Title = "Home";
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
            if(firstName == "" || lastName == "" || uName == "" || title == "" || cellNo == "" || email == "" || pAddress == "" || country == "" || city == "" || zip == "" || dob == "" || empStatus == "" || password =="" || confPass == "")
            {
                return RedirectToAction("registerScreen");
            }// checks that fileds are not empty
            if (password != confPass)
            {
                return RedirectToAction("registerScreen");
            }// checks that passwords match
            else
            {
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
                if(countryIndex == -1)
                {
                    Country newCountry = new Country();
                    newCountry.Country_ID = cList.Count() + 1;
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
                    newCity.City_ID = cityList.Count() + 1;
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
                    newZip.Zip_ID = zList.Count() + 1;
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
                newPassword.Password_ID = passList.Count + 1;
                string encPassword = encodePassword(confPass);
                newPassword.Encrypted_password = encPassword;
                newPassword.Date_Set = DateTime.Now;
                passList.Add(newPassword);
                db.UserPasswords.Add(newPassword);
                db.SaveChanges();
                user.Password_ID = newPassword.Password_ID;
                user.SysUser_ID = db.Sys_User.ToList().Count + 1;
                db.Sys_User.Add(user);
                db.SaveChanges();

                //create email
                MimeMessage requestEmail = new MimeMessage();
                requestEmail.From.Add(new MailboxAddress("New user", "danielmarcstewart@gmail.com"));
                requestEmail.To.Add(MailboxAddress.Parse("danielmarcstewart@gmail.com"));
                requestEmail.Subject = "New user request";
                requestEmail.Body = new TextPart("plain") { Text = "A new user wishes to be requstered on the system: User name: " + firstName + " " + lastName  + " email address: " + email};

                //send email
                SmtpClient client = new SmtpClient();
                client.Connect("smtp.gmail.com", 465, true);
                client.Authenticate("danielmarcstewart@gmail.com", "Titan0208");
                client.Send(requestEmail);
                client.Disconnect(true);
                client.Dispose();
                return RedirectToAction("registrationConformationScreen");
            }// adds user, emails admin
            
        }// registering users

        public ActionResult registrationConformationScreen(string uName, string code)
        {
            if (uName == "" || code == "")
            {
                return View();
            }// student 
            if (code == "101")
            {
                string studentLicence = Request["licence"];
                Student newStudent = new Student();
                int studentID = db.Students.ToList().Count + 1;
                newStudent.Student_ID = studentID;
                newStudent.Licence_No = Convert.ToInt32(studentLicence);

                Sys_User newStudentUser = db.Sys_User.ToList().Find(u => u.User_Name == uName);
                newStudentUser.User_Role_ID = 2;
                db.Entry(newStudentUser).State = System.Data.Entity.EntityState.Modified;
                db.SaveChangesAsync();
                newStudent.SysUser_ID = newStudentUser.SysUser_ID;
                db.Students.Add(newStudent);
                db.SaveChangesAsync();
                return RedirectToAction("loginScreen");
            }// student 
            if (code == "102")
            {
                string instructorLicence = Request["licence"];
                Instructor newInstructor = new Instructor();
                int indtructorId = db.Instructors.ToList().Count + 1;
                newInstructor.Instructor_ID = indtructorId;
                newInstructor.Licence_No = Convert.ToInt32(instructorLicence);

                Sys_User newInsUser = db.Sys_User.ToList().Find(u => u.User_Name == uName);
                newInsUser.User_Role_ID = 2;
                db.Entry(newInsUser).State = System.Data.Entity.EntityState.Modified;
                db.SaveChangesAsync();
                newInstructor.SysUser_ID = newInsUser.SysUser_ID;
                db.Instructors.Add(newInstructor);
                db.SaveChangesAsync();
                return RedirectToAction("loginScreen");
            }// instructor
            if (code == "103")
            {
                Admin newAdmin = new Admin();
                int adminId = db.Admins.ToList().Count + 1;
                newAdmin.Admin_ID = adminId;


                Sys_User newAdminUser = db.Sys_User.ToList().Find(u => u.User_Name == uName);
                newAdminUser.User_Role_ID = 2;
                db.Entry(newAdminUser).State = System.Data.Entity.EntityState.Modified;
                db.SaveChangesAsync();
                newAdmin.SysUser_ID = newAdminUser.SysUser_ID;
                db.Admins.Add(newAdmin);
                db.SaveChangesAsync();
                return RedirectToAction("loginScreen");
            }// admin
            if (code == "104")
            {
                Manager newManager = new Manager();
                int managerId = db.Managers.ToList().Count + 1;
                newManager.Manager_ID = managerId;

                Sys_User newManagerUser = db.Sys_User.ToList().Find(u => u.User_Name == uName);
                newManagerUser.User_Role_ID = 2;
                db.Entry(newManagerUser).State = System.Data.Entity.EntityState.Modified;
                db.SaveChangesAsync();
                newManager.SysUser_ID = newManagerUser.SysUser_ID;
                db.Managers.Add(newManager);
                db.SaveChangesAsync();
                return RedirectToAction("loginScreen");
            }// manager 
            else
            {
                return View();
            }
        }// finalise registration with codes

        public ActionResult viewAccount(int? id)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
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

        }// view account function

        public ActionResult searchScreen(int? id)
        {
            Sys_User viewUser = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
            return View(viewUser);
        }// search screen

        public ActionResult searchResultsScreen(int? id, string firstName, string lastName, string list)
        {
            ViewData["userID"] = "" + id;
            ViewData["role"] = list;
            List<Sys_User> sUsers = new List<Sys_User>();
            if (firstName == "" && lastName == "")
            {
                return RedirectToAction("searchScreen", new { id = id });
            }// if fields are empty
            else
            {
                if(list == "student")
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
        }// displays search results

        public ActionResult updateAccount(int? id)
        {
            Sys_User updateUser = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
            return View(updateUser);
        }// return update screen

        [HttpPost]
        public ActionResult updateAccount(string id, string firstName, string lastName, string uName, string title, string cellNo, string email, string pAddress, string country, string city, string zip, string dob, string empStatus)
        {
            int idUser = Convert.ToInt32(id);
            Sys_User sys_User = db.Sys_User.ToList().Find(u => u.SysUser_ID == idUser);
            if (firstName == "" || lastName == "" || uName == "" || title == "" || cellNo == "" || email == "" || pAddress == "" || country == "" || city == "" || zip == "" || dob == "" || empStatus == "")
            {
                return RedirectToAction("updateAccount");
            }// checks that fileds are not empty
            else
            {
                Sys_User updateUser = new Sys_User();
                DateTime DOB = DateTime.Parse(dob);
                updateUser.SysUser_ID = idUser;
                updateUser.User_Role_ID = sys_User.User_Role_ID;
                updateUser.Password_ID = sys_User.Password_ID;
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
                    newCountry.Country_ID = cList.Count() + 1;
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
                    newCity.City_ID = cityList.Count() + 1;
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
                    newZip.Zip_ID = zList.Count() + 1;
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

                db.Sys_User.Remove(sys_User);
                db.SaveChanges();

                db.Sys_User.Add(updateUser);
                db.SaveChanges();

                return RedirectToAction("homeScreen", new { id = idUser });
            }// adds user, emails admin

        }// updating users

        public ActionResult addStudentResource(int? loggedId, int? id)
        {
            ViewData["userID"] = "" + loggedId;
            ViewData["studentID"] = "" + id;
            return View(db.Study_Resource.ToList());
        }// add student resource get

        [HttpPost]
        public ActionResult addStudentResource(int? loggedId, int? id, string resource)
        {
            try
            {
                int resourceId = Convert.ToInt32(resource);
                Student_Resource newStudentResource = new Student_Resource();
                int sID = Convert.ToInt32(id);
                newStudentResource.Student_ID = sID;
                newStudentResource.Study_Resource_ID = resourceId;
                db.Student_Resource.Add(newStudentResource);
                db.SaveChanges();
                return RedirectToAction("searchScreen", new { id = loggedId });
            }// try
            catch(Exception ex)
            {
                return RedirectToAction("searchScreen", new { id = loggedId });
            }// catch
        }// add student resource post

        public ActionResult addStudentInstructor(int? loggedId, int? id)
        {
            ViewData["userID"] = "" + loggedId;
            ViewData["studentID"] = "" + id;
            List<Sys_User> instructors = db.Sys_User.ToList().FindAll(i => i.User_Role_ID == 2);
            return View(instructors);
        }// add student instructor get

        [HttpPost]
        public ActionResult addStudentInstructor(int? loggedId, int? id, string insturctor)
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
                List<Lesson_Plan> planList = db.Lesson_Plan.ToList().FindAll(p => p.Instructor_ID == ins.SysUser_ID);
                foreach(Lesson_Plan temp in planList)
                {
                    Student_Lesson_Plan sLessonPlan = new Student_Lesson_Plan();
                    sLessonPlan.Student_ID = stu.SysUser_ID;
                    sLessonPlan.Lesson_Plan_ID = temp.Lesson_Plan_ID;
                    db.Student_Lesson_Plan.Add(sLessonPlan);
                    db.SaveChangesAsync();
                }// for each
                return RedirectToAction("searchScreen", new { id = loggedId });
            }// try
            catch (Exception ex)
            {
                return RedirectToAction("searchScreen", new { id = loggedId });
            }// catch
        }// add student instructor post

        public ActionResult resetPassword(int? id)
        {
            Sys_User passwordUser = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
            return View(passwordUser);
        }// reset password

        [HttpPost]
        public ActionResult resetPassword(int? id, string oldPass, string newPass, string confPass)
        {
            if(oldPass == "" || newPass == "" || confPass == "")
            {
                return View (db.Sys_User.ToList().Find(u => u.SysUser_ID == id));
            }// if fields are empty
            if(newPass != confPass)
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
                if(decPass == oldPass)
                {
                    string encNewPass = encodePassword(confPass);
                    pass.Encrypted_password = encNewPass;
                    pass.Date_Set = DateTime.Now;
                    db.Entry(pass).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChangesAsync();
                }// if entered password matches db password
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
            if (userName == "" || email =="" || newPass == "" || confPass == "")
            {
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
            ViewData["loggedId"] = "" + loggedId;
            Sys_User delUser = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
            return View(delUser);
        }// delete get

        public ActionResult deleteConformation(int? loggedId, int? id)
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
                db.Student_Instructor.Remove(delStudentInstructor);
                db.SaveChanges();
                Student_Resource delStuResource = db.Student_Resource.ToList().Find(s => s.Student_ID == id);
                db.Student_Resource.Remove(delStuResource);
                db.SaveChanges();
                Student_Lesson_Plan delStuPlan = db.Student_Lesson_Plan.ToList().Find(s => s.Student_ID == id);
                db.Student_Lesson_Plan.Remove(delStuPlan);
                db.SaveChanges();
            }// students
            if (sys_User.User_Role_ID == 2)
            {
                List<Instructor> tempInstructorList = db.Instructors.ToList();
                Instructor delInstructor = tempInstructorList.Find(u => u.SysUser_ID == id);
                db.Instructors.Remove(delInstructor);
                db.SaveChanges();
                Student_Instructor delStudentInstructor = db.Student_Instructor.ToList().Find(s => s.Instructor_ID == delInstructor.Instructor_ID);
                db.Student_Instructor.Remove(delStudentInstructor);
                db.SaveChanges();
                List<Lesson_Plan> planList = db.Lesson_Plan.ToList().FindAll(i => i.Instructor_ID == id);
                foreach(Lesson_Plan temp in planList)
                {
                    db.Lesson_Plan.Remove(temp);
                    db.SaveChangesAsync();
                }
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
            return RedirectToAction("homeScreen", new { id = loggedId });
        }

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
            catch(Exception ex)
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
