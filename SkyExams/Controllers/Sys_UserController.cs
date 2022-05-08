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
//using System.Net.Mail;
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

        public ActionResult homeScreen()
        {
            string uName = Request["userName"];
            string pass = Request["password"];
            string passwordFromDb = "";
            Sys_User loginUser = new Sys_User();
            if (uName != "" || pass != "")
            {
                foreach (Sys_User tempUser in db.Sys_User.ToList())
                {
                    if(uName == tempUser.User_Name)
                    {
                        loginUser = tempUser;
                        foreach(UserPassword tempPass in db.UserPasswords.ToList())
                        {
                            if(loginUser.Password_ID == tempPass.Password_ID)
                            {
                                passwordFromDb = decodePassword(tempPass.Encrypted_password);
                                if(pass == passwordFromDb)
                                {
                                    return View(loginUser);
                                }// checks is entered password matches db password
                            }// matches user and password ids
                        }// searches passwords
                    }// matches username to db username
                }// searches users
                return RedirectToAction("loginScreen");
            }
            else
            {
                return RedirectToAction("loginScreen");
            }
            // verify username and password here, if correct then display home screen, else login screen with a pop up
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
                requestEmail.From.Add(new MailboxAddress("New user", "u20428660@tuks.co.za"));
                requestEmail.To.Add(MailboxAddress.Parse("u20428660@tuks.co.za"));
                requestEmail.Subject = "New user request";
                requestEmail.Body = new TextPart("plain") { Text = "A new user wishes to be requstered on the system" + firstName + " " + lastName };

                //send email
                SmtpClient client = new SmtpClient();
                client.Connect("smtp.gmail.com", 465, true);
                client.Authenticate("u20428660@tuks.co.za", "Titan0208#");
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
                db.SaveChanges();
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
                db.SaveChanges();
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
                db.SaveChanges();
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
                db.SaveChanges();
                return RedirectToAction("loginScreen");
            }// manager 
            else
            {
                return View();
            }
        }// finalise registration with codes

        public ActionResult viewAccount(int? id)
        {
            Sys_User viewUser = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
            return View(viewUser);
        }// view account function

        public ActionResult searchScreen(int? id)
        {
            Sys_User viewUser = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
            return View(viewUser);
        }// search screen

        public ActionResult searchResultsScreen(int? id, string firstName, string lastName, string list)
        {
            List<Sys_User> searchedUsers = new List<Sys_User>();
            if (firstName == "" && lastName == "")
            {
                return RedirectToAction("searchScreen");
            }// if fields are empty
            else
            {
                if(list == "student")
                {
                    searchedUsers = db.Sys_User.ToList().FindAll(u => u.User_Role_ID == 1 && u.FName == firstName || u.Surname == lastName);
                }// students
                if (list == "instructor")
                {
                    searchedUsers = db.Sys_User.ToList().FindAll(u => u.User_Role_ID == 2 && u.FName == firstName || u.Surname == lastName);
                }// instructors
                if (list == "admin")
                {
                    searchedUsers = db.Sys_User.ToList().FindAll(u => u.User_Role_ID == 3 && u.FName == firstName || u.Surname == lastName);
                }// admin
                if (list == "manager")
                {
                    searchedUsers = db.Sys_User.ToList().FindAll(u => u.User_Role_ID == 4 && u.FName == firstName || u.Surname == lastName);
                }// manager
            }// fields arent empty
            return View(searchedUsers);
        }// displays search results

        public ActionResult updateAccount(int? id)
        {
            Sys_User updateUser = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
            return View(updateUser);
        }// return register screen

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
                DateTime DOB = DateTime.Parse(dob);
                sys_User.FName = firstName;
                sys_User.Surname = lastName;
                sys_User.Cell_Number = cellNo;
                sys_User.Email_Address = email;
                sys_User.Physical_Address = pAddress;
                sys_User.DOB = DOB;
                sys_User.User_Name = uName;

                List<Country> cList = db.Countries.ToList();
                int countryIndex = -1;
                countryIndex = cList.FindIndex(c => c.Country_Name == country);
                if (countryIndex == -1)
                {
                    Country newCountry = new Country();
                    newCountry.Country_ID = cList.Count() + 1;
                    newCountry.Country_Name = country;
                    cList.Add(newCountry);
                    sys_User.Country_ID = newCountry.Country_ID;
                    db.Countries.Add(newCountry);
                    db.SaveChanges();
                }//if country dosent exist
                else
                {
                    sys_User.Country_ID = cList[countryIndex].Country_ID;
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
                    sys_User.City_ID = newCity.City_ID;
                    db.Cities.Add(newCity);
                    db.SaveChanges();
                }// if city dosent exist
                else
                {
                    sys_User.City_ID = cityList[cityIndex].City_ID;
                }// if city exists

                List<Title> tList = db.Titles.ToList();
                foreach (Title tempTitle in db.Titles.ToList())
                {
                    if (title == tempTitle.TitleDesc)
                    {
                        sys_User.Title_ID = tempTitle.Title_ID;
                    }// if title exists
                }// loops through all titles, if the title exists, is used that ID for the user, if not, it creates a new title and uses that ID

                List<Employment_Status> eList = db.Employment_Status.ToList();
                foreach (Employment_Status tempStatus in db.Employment_Status.ToList())
                {
                    if (empStatus == tempStatus.EmpStatus)
                    {
                        sys_User.Employment_ID = tempStatus.Employment_ID;
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
                    sys_User.ZIP_ID = newZip.Zip_ID;
                    db.Zip_Code.Add(newZip);
                    db.SaveChanges();
                }// if zip dosent exist
                else
                {
                    sys_User.ZIP_ID = zList[zipIndex].Zip_ID;
                }// if zip exists


                db.Entry(sys_User).State = System.Data.Entity.EntityState.Modified;
                db.SaveChangesAsync();

                return RedirectToAction("loginScreen");
            }// adds user, emails admin

        }// updating users

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
                throw new Exception("Error in base64Encode");
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
