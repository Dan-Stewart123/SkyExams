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
            Sys_User user = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
            return View(user);
        }// returns plane screen

        public ActionResult addPlaneScreen(int? id)
        {
            Sys_User user = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
            return View(user);
        }// get for add plane screen

        [HttpPost]
        public ActionResult addPlaneScreen(int id, string type, string description, string sign, int? hoursFlown, int? serviceHours)
        {
            if(type == "" || description == "" || sign == "" || hoursFlown == 0 || serviceHours == 0)
            {
                return RedirectToAction("addPlaneScreen", new { id = id });
            }// checks fields arent empty
            else
            {
                Plane newPlane = new Plane();
                List<Plane> planeList = new List<Plane>();
                int planeID = planeList.Count() + 2;
                newPlane.Plane_ID = planeID;
                newPlane.Call_Sign = sign;
                newPlane.Hours_Flown = hoursFlown;
                newPlane.Hours_Until_Service = serviceHours;
                if(type == "1")
                {
                    Cessna_172 new172 = new Cessna_172();
                    List<Cessna_172> cessna172List = new List<Cessna_172>();
                    int tempID = cessna172List.Count() + 2;
                    new172.c172_ID = tempID;
                    new172.Plane_ID = planeID;
                    new172.Plane_Description = description;
                    db.Cessna_172.Add(new172);
                    db.SaveChanges();
                    newPlane.Plane_Type_ID = 1;
                }// cessna 172
                if (type == "2")
                {
                    Cessna_172_RG newRG = new Cessna_172_RG();
                    List<Cessna_172_RG> rgList = new List<Cessna_172_RG>();
                    int tempID = rgList.Count() + 2;
                    newRG.c172_RG_ID = tempID;
                    newRG.Plane_ID = planeID;
                    newRG.Plane_Description = description;
                    db.Cessna_172_RG.Add(newRG);
                    db.SaveChanges();
                    newPlane.Plane_Type_ID = 2;
                }// cessna 172 RG
                if (type == "3")
                {
                    Cherokee_140 new140 = new Cherokee_140();
                    List<Cherokee_140> cherokeeList = new List<Cherokee_140>();
                    int tempID = cherokeeList.Count() + 2;
                    new140.Cherokee_140_ID = tempID;
                    new140.Plane_ID = planeID;
                    new140.Plane_Description = description;
                    db.Cherokee_140.Add(new140);
                    db.SaveChanges();
                    newPlane.Plane_Type_ID = 3;
                }// cherokee 140
                if (type == "4")
                {
                    Twin_Commanche newTwin = new Twin_Commanche();
                    List<Twin_Commanche> twinList = new List<Twin_Commanche>();
                    int tempID = twinList.Count() + 2;
                    newTwin.Twin_Commanche_ID = tempID;
                    newTwin.Plane_ID = planeID;
                    newTwin.Plane_Description = description;
                    db.Twin_Commanche.Add(newTwin);
                    db.SaveChanges();
                    newPlane.Plane_Type_ID = 4;
                }// twin commanche
                db.Planes.Add(newPlane);
                db.SaveChanges();
                Sys_User user = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
                return RedirectToAction("planesScreen", new { id = id });
            }// if fields are valid
        }// post for add plane screen

        public ActionResult planeTypeScreen(int? id, int? typeId)
        {
            ViewData["userID"] = "" + id;
            List<Plane> planeList = new List<Plane>();
            if(typeId == 1)
            {
                ViewData["planeType"] = "Cessna 172";
                planeList = db.Planes.ToList().FindAll(p => p.Plane_Type_ID == 1);
                return View(planeList);
            }// cessna 172
            if (typeId == 2)
            {
                ViewData["planeType"] = "Cessna 172 RG";
                planeList = db.Planes.ToList().FindAll(p => p.Plane_Type_ID == 2);
                return View(planeList);
            }// cessna 172 RG
            if (typeId == 3)
            {
                ViewData["planeType"] = "Cherokee 140";
                planeList = db.Planes.ToList().FindAll(p => p.Plane_Type_ID == 3);
                return View(planeList);
            }// cherokee 140
            if (typeId == 4)
            {
                ViewData["planeType"] = "Piper Twin Comanche";
                planeList = db.Planes.ToList().FindAll(p => p.Plane_Type_ID == 4);
                return View(planeList);
            }// twin commanche
            else
            {
                return View(planeList);
            }
        }// plane type screen

        public ActionResult deletePlane(int? loggedId, int? id)
        {
            ViewData["loggedId"] = "" + loggedId;
            Plane delPlane = db.Planes.ToList().Find(p => p.Plane_ID == id);
            return View(delPlane);
        }// delete plane

        public ActionResult deletePlaneConformation(int? loggedId, int? id)
        {
            Plane delPlane = db.Planes.Find(id);
            int planeType = delPlane.Plane_Type_ID;
            int planeID = delPlane.Plane_ID;
            db.Planes.Remove(delPlane);
            db.SaveChanges();
            if(planeType == 1)
            {
                Cessna_172 del172 = db.Cessna_172.ToList().Find(p => p.Plane_ID == planeID);
                db.Cessna_172.Remove(del172);
                db.SaveChanges();
            }// cessna 172
            if (planeType == 2)
            {
                Cessna_172_RG delRG = db.Cessna_172_RG.ToList().Find(p => p.Plane_ID == planeID);
                db.Cessna_172_RG.Remove(delRG);
                db.SaveChanges();
            }// cessna 172 RG
            if (planeType == 3)
            {
                Cherokee_140 del140 = db.Cherokee_140.ToList().Find(p => p.Plane_ID == planeID);
                db.Cherokee_140.Remove(del140);
                db.SaveChanges();
            }// Cherokee 140
            if (planeType == 4)
            {
                Twin_Commanche delTwin = db.Twin_Commanche.ToList().Find(p => p.Plane_ID == planeID);
                db.Twin_Commanche.Remove(delTwin);
                db.SaveChanges();
            }// Twin Commanche
            return RedirectToAction("planeTypeScreen", new { id = loggedId, typeId = planeType });
        }// delete plane conformation

        public ActionResult updatePlane(int? loggedId, int? id)
        {
            ViewData["loggedId"] = "" + loggedId;
            Plane updatePlane = db.Planes.ToList().Find(p => p.Plane_ID == id);
            return View(updatePlane);
        }// update plane get

        [HttpPost]
        public ActionResult updatePlane(string userId, string id, string description, string sign, int? hoursFlown, int? serviceHours)
        {
            int pID = Convert.ToInt32(id);
            Plane plane = db.Planes.Find(pID);
            if(description == "" || sign == "" || hoursFlown == 0 || serviceHours == 0)
            {
                return RedirectToAction("updatePlane");
            }// checks if all fields are complete 
            else
            {
                Plane updatePlane = new Plane();
                updatePlane.Plane_ID = plane.Plane_ID;
                updatePlane.Plane_Type_ID = plane.Plane_Type_ID;
                updatePlane.Call_Sign = sign;
                updatePlane.Hours_Flown = hoursFlown;
                updatePlane.Hours_Until_Service = serviceHours;
                if(plane.Plane_Type_ID == 1)
                {
                    Cessna_172 c172 = db.Cessna_172.ToList().Find(p => p.Plane_ID == updatePlane.Plane_ID);
                    Cessna_172 update172 = new Cessna_172();
                    update172.Plane_Description = description;
                    update172.c172_ID = c172.c172_ID;
                    update172.Plane_ID = c172.Plane_ID;

                    db.Cessna_172.Remove(c172);
                    db.SaveChanges();

                    db.Cessna_172.Add(update172);
                    db.SaveChanges();
                }// cessna 172

                if (plane.Plane_Type_ID == 2)
                {
                    Cessna_172_RG rg = db.Cessna_172_RG.ToList().Find(p => p.Plane_ID == updatePlane.Plane_ID);
                    Cessna_172_RG updateRg = new Cessna_172_RG();
                    updateRg.Plane_Description = description;
                    updateRg.c172_RG_ID = rg.c172_RG_ID;
                    updateRg.Plane_ID = rg.Plane_ID;

                    db.Cessna_172_RG.Remove(rg);
                    db.SaveChanges();

                    db.Cessna_172_RG.Add(updateRg);
                    db.SaveChanges();
                }// cessna 172 RG

                if (plane.Plane_Type_ID == 3)
                {
                    Cherokee_140 c140 = db.Cherokee_140.ToList().Find(p => p.Plane_ID == updatePlane.Plane_ID);
                    Cherokee_140 update140 = new Cherokee_140();
                    update140.Plane_Description = description;
                    update140.Cherokee_140_ID = c140.Cherokee_140_ID;
                    update140.Plane_ID = c140.Plane_ID;

                    db.Cherokee_140.Remove(c140);
                    db.SaveChanges();

                    db.Cherokee_140.Add(update140);
                    db.SaveChanges();
                }// cherokee 140

                if (plane.Plane_Type_ID == 4)
                {
                    Twin_Commanche twin = db.Twin_Commanche.ToList().Find(p => p.Plane_ID == updatePlane.Plane_ID);
                    Twin_Commanche updateTwin = new Twin_Commanche();
                    updateTwin.Plane_Description = description;
                    updateTwin.Twin_Commanche_ID = twin.Twin_Commanche_ID;
                    updateTwin.Plane_ID = twin.Plane_ID;

                    db.Twin_Commanche.Remove(twin);
                    db.SaveChanges();

                    db.Twin_Commanche.Add(updateTwin);
                    db.SaveChanges();
                }// twin commanche

                db.Planes.Remove(plane);
                db.SaveChanges();

                db.Planes.Add(updatePlane);
                db.SaveChanges();

                int uID = Convert.ToInt32(userId);
                return RedirectToAction("planeTypeScreen", new { id = uID, typeId = updatePlane.Plane_Type_ID });
            }// else
        }// update plane post

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
