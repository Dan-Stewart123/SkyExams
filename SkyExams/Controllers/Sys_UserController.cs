using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SkyExams.Models;

namespace SkyExams.Controllers
{
    public class Sys_UserController : Controller
    {
        private SkyExamsEntities db = new SkyExamsEntities();

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
            //find that users password
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
