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
            //search for user based in user name here
            //find that users password and store them on a global private student variable
            // remember to decrypt the passwords
            if(uName == "" && pass == "")
            {
                return RedirectToAction("loginScreen");
            }
            else
            {
                return View();
            }
            // verify username and password here, if correct then display home screen, else login screen with a pop up
        }// returns home screen

        public ActionResult registerScreen()
        {
            return View();
        }// return register screen

        [HttpPost]
        public ActionResult registerScreen(string firstName)
        {
            string fName = Request["firstName"];
            string lName = Request["lastName"];
            string uName = Request["uName"];
            string title = Request["title"];
            string cellNo = Request["cellNo"];
            string email = Request["email"];
            string pAddress = Request["pAddress"];
            string country = Request["country"];
            string city = Request["city"];
            string zip = Request["zip"];
            //profile image
            string tempDate = Request["dob"];
            string empStatus = Request["empStatus"];
            string password = Request["password"];
            string confirmPassword = Request["confPass"];
            if(fName == "" || lName == "" || uName == "" || title == "" || cellNo == "" || email == "" || pAddress == "" || country == "" || city == "" || zip == "" || tempDate == "" || empStatus == "" || password =="" || confirmPassword == "")
            {
                return RedirectToAction("registerScreen");
            }// checks that fileds are not empty
            if (password != confirmPassword)
            {
                return RedirectToAction("registerScreen");
            }// checks that passwords match
            else
            {
                DateTime dob = DateTime.Parse(tempDate);
                user.FName = fName;
                user.Surname = lName;
                user.Cell_Number = cellNo;
                user.Email_Address = email;
                user.Physical_Address = pAddress;
                user.DOB = dob;
                List<Country> cList = new List<Country>();
                cList = db.Countries.ToList();
                foreach(Country tempCountry in db.Countries.ToList())
                {
                    if (country == tempCountry.Country_Name)
                    {
                        user.Country_ID = tempCountry.Country_ID;
                    }// if country exists
                    else
                    {
                        Country newCountry = new Country();
                        newCountry.Country_ID = cList.Count() + 1;
                        newCountry.Country_Name = country;
                        cList.Add(newCountry);
                        user.Country_ID = newCountry.Country_ID;
                    }// if country dosent exist
                }// loops through all countries, if the country exists, is used that ID for the user, if not, it creates a new country and uses that ID

                List<City> cityList = new List<City>();
                cityList = db.Cities.ToList();
                foreach (City tempCity in db.Cities.ToList())
                {
                    if (city == tempCity.City_Name)
                    {
                        user.City_ID = tempCity.City_ID;
                    }// if City exists
                    else
                    {
                        City newCity = new City();
                        newCity.City_ID = cityList.Count() + 1;
                        newCity.City_Name = country;
                        cityList.Add(newCity);
                        user.Country_ID = newCity.City_ID;
                    }// if City dosent exist
                }// loops through all cities, if the city exists, is used that ID for the user, if not, it creates a new city and uses that ID

                List<Title> tList = new List<Title>();
                tList = db.Titles.ToList();
                foreach (Title tempTitle in db.Titles.ToList())
                {
                    if (title == tempTitle.TitleDesc)
                    {
                        user.City_ID = tempTitle.Title_ID;
                    }// if title exists
                    else
                    {
                        Title newTitle = new Title();
                        newTitle.Title_ID = tList.Count() + 1;
                        newTitle.TitleDesc = title;
                        tList.Add(newTitle);
                        user.Country_ID = newTitle.Title_ID;
                    }// if title dosent exist
                }// loops through all titles, if the title exists, is used that ID for the user, if not, it creates a new title and uses that ID

                List<Employment_Status> eList = new List<Employment_Status>();
                eList = db.Employment_Status.ToList();
                foreach (Employment_Status tempStatus in db.Employment_Status.ToList())
                {
                    if (empStatus == tempStatus.EmpStatus)
                    {
                        user.Employment_ID = tempStatus.Employment_ID;
                    }// if status exists
                    else
                    {
                        Employment_Status newStatus = new Employment_Status();
                        newStatus.Employment_ID = eList.Count() + 1;
                        newStatus.EmpStatus = empStatus;
                        eList.Add(newStatus);
                        user.Country_ID = newStatus.Employment_ID;
                    }// if status dosent exist
                }// loops through all statuses, if the status exists, is used that ID for the user, if not, it creates a new status and uses that ID

                List<Zip_Code> zList = new List<Zip_Code>();
                zList = db.Zip_Code.ToList();
                foreach (Zip_Code tempZip in db.Zip_Code.ToList())
                {
                    if (zip == tempZip.Code)
                    {
                        user.ZIP_ID = tempZip.Zip_ID;
                    }// if zip exists
                    else
                    {
                        Zip_Code newZip = new Zip_Code();
                        newZip.Zip_ID = eList.Count() + 1;
                        newZip.Code = zip;
                        zList.Add(newZip);
                        user.Country_ID = newZip.Zip_ID;
                    }// if zip dosent exist
                }// loops through all codes, if the code exists, is used that ID for the user, if not, it creates a new code and uses that ID

                List<UserPassword> passList = new List<UserPassword>();
                UserPassword newPassword = new UserPassword();
                newPassword.Password_ID = passList.Count + 1;
                string encPassword = encodePassword(confirmPassword);
                newPassword.Encrypted_password = encPassword;
                newPassword.Date_Set = DateTime.Now;
                passList.Add(newPassword);
                user.Password_ID = newPassword.Password_ID;

                //create email
                var requestEmail = new MimeMessage();
                requestEmail.From.Add(MailboxAddress.Parse("uriah.cronin5@ethereal.email"));
                requestEmail.To.Add(MailboxAddress.Parse("u20428660@tuks.co.za"));
                requestEmail.Subject = "New user request";
                requestEmail.Body = new TextPart(MimeKit.Text.TextFormat.Plain) { Text = "A new user wishes to be requstered on the system" };

                //send email
                var smtp = new SmtpClient();
                smtp.Connect("smtp.ethereal.email", 587, MailKit.Security.SecureSocketOptions.StartTls);
                smtp.Authenticate("uriah.cronin5@ethereal.email", "SGhYKTAadrj6kcNxF5");
                smtp.Send(requestEmail);
                smtp.Disconnect(true);
                return RedirectToAction("registrationConformationScreen");
            }// adds user, emails admin
            
        }// registering users

        public ActionResult registrationConformationScreen(string code)
        {
            if (code == "")
            {
                return View();
            }// student 
            if (code == "101")
            {
                string studentLicence = Request["licence"];
                Student newStudent = new Student();
                int studentID = db.Students.ToList().Count + 1;
                newStudent.Student_ID = studentID;
                newStudent.SysUser_ID = user.SysUser_ID;
                newStudent.Licence_No = Convert.ToInt32(studentLicence);
                // add to db
                return RedirectToAction("loginScreen");
            }// student 
            if (code == "102")
            {
                string instructorLicence = Request["licence"];
                Instructor inewInstructor = new Instructor();
                int indtructorId = db.Instructors.ToList().Count + 1;
                inewInstructor.Instructor_ID = indtructorId;
                inewInstructor.SysUser_ID = user.SysUser_ID;
                inewInstructor.Licence_No = Convert.ToInt32(instructorLicence);
                // add to db
                return RedirectToAction("loginScreen");
            }// instructor
            if (code == "103")
            {
                Admin newAdmin = new Admin();
                int adminId = db.Admins.ToList().Count + 1;
                newAdmin.Admin_ID = adminId;
                newAdmin.SysUser_ID = user.SysUser_ID;
                return RedirectToAction("loginScreen");
            }// admin
            if (code == "104")
            {
                Manager newManager = new Manager();
                int managerId = db.Managers.ToList().Count + 1;
                newManager.Manager_ID = managerId;
                newManager.SysUser_ID = user.SysUser_ID;
                return RedirectToAction("loginScreen");
            }// manager 
            else
            {
                return View();
            }
        }// finalise registration with codes

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
