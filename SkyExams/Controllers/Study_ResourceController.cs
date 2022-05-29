using SkyExams.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace SkyExams.Controllers
{
    public class Study_ResourceController : Controller
    {
        private SkyExamsEntities db = new SkyExamsEntities();

        // GET: Study_Resource
        public ActionResult Index()
        {
            return View(db.Study_Resource.ToList());
        }

        public ActionResult resourceScreen(int? id)
        {
            Sys_User user = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
            return View(user);
        }// returns resource screen

        public ActionResult themeScreen(int? id, int? typeId)
        {
            ViewData["userID"] = "" + id;
            Sys_User user = db.Sys_User.Find(id);
            int userRole = Convert.ToInt32(user.User_Role_ID);
            List<Study_Resource> resourceList = new List<Study_Resource>();
            if(userRole == 1)
            {
                if (typeId == 1)
                {
                    ViewData["themeID"] = "" + 1;
                    ViewData["planeType"] = "Cessna 172";
                    List<Student_Resource> stuResource = db.Student_Resource.ToList().FindAll(r => r.Student_ID == user.SysUser_ID);
                    foreach(Student_Resource temp in stuResource)
                    {
                        List<Study_Resource> tempResources = db.Study_Resource.ToList();
                        foreach(Study_Resource teResource in tempResources)
                        {
                            if(teResource.Study_Resource_ID == temp.Study_Resource_ID && teResource.Theme_ID == 1)
                            {
                                resourceList.Add(teResource);
                            }
                        }// inner for each
                    }// for each
                    return View(resourceList);
                }// cessna 172
                if (typeId == 2)
                {
                    ViewData["themeID"] = "" + 2;
                    ViewData["planeType"] = "Cessna 172 RG";
                    List<Student_Resource> stuResource = db.Student_Resource.ToList().FindAll(r => r.Student_ID == user.SysUser_ID);
                    foreach (Student_Resource temp in stuResource)
                    {
                        List<Study_Resource> tempResources = db.Study_Resource.ToList();
                        foreach (Study_Resource teResource in tempResources)
                        {
                            if (teResource.Study_Resource_ID == temp.Study_Resource_ID && teResource.Theme_ID == 2)
                            {
                                resourceList.Add(teResource);
                            }
                        }// inner for each
                    }// for each
                    return View(resourceList);
                }// cessna 172 RG
                if (typeId == 3)
                {
                    ViewData["themeID"] = "" + 3;
                    ViewData["planeType"] = "Cherokee 140";
                    List<Student_Resource> stuResource = db.Student_Resource.ToList().FindAll(r => r.Student_ID == user.SysUser_ID);
                    foreach (Student_Resource temp in stuResource)
                    {
                        List<Study_Resource> tempResources = db.Study_Resource.ToList();
                        foreach (Study_Resource teResource in tempResources)
                        {
                            if (teResource.Study_Resource_ID == temp.Study_Resource_ID && teResource.Theme_ID == 3)
                            {
                                resourceList.Add(teResource);
                            }
                        }// inner for each
                    }// for each
                    return View(resourceList);
                }// cherokee 140
                if (typeId == 4)
                {
                    ViewData["themeID"] = "" + 4;
                    ViewData["planeType"] = "Piper Twin Comanche";
                    List<Student_Resource> stuResource = db.Student_Resource.ToList().FindAll(r => r.Student_ID == user.SysUser_ID);
                    foreach (Student_Resource temp in stuResource)
                    {
                        List<Study_Resource> tempResources = db.Study_Resource.ToList();
                        foreach (Study_Resource teResource in tempResources)
                        {
                            if (teResource.Study_Resource_ID == temp.Study_Resource_ID && teResource.Theme_ID == 4)
                            {
                                resourceList.Add(teResource);
                            }
                        }// inner for each
                    }// for each
                    return View(resourceList);
                }// twin commanche
                else
                {
                    return View(resourceList);
                }
            }// if user is a student
            else
            {
                if (typeId == 1)
                {
                    ViewData["themeID"] = "" + 1;
                    ViewData["planeType"] = "Cessna 172";
                    resourceList = db.Study_Resource.ToList().FindAll(p => p.Theme_ID == 1);
                    return View(resourceList);
                }// cessna 172
                if (typeId == 2)
                {
                    ViewData["themeID"] = "" + 2;
                    ViewData["planeType"] = "Cessna 172 RG";
                    resourceList = db.Study_Resource.ToList().FindAll(p => p.Theme_ID == 2);
                    return View(resourceList);
                }// cessna 172 RG
                if (typeId == 3)
                {
                    ViewData["themeID"] = "" + 3;
                    ViewData["planeType"] = "Cherokee 140";
                    resourceList = db.Study_Resource.ToList().FindAll(p => p.Theme_ID == 3);
                    return View(resourceList);
                }// cherokee 140
                if (typeId == 4)
                {
                    ViewData["themeID"] = "" + 4;
                    ViewData["planeType"] = "Piper Twin Comanche";
                    resourceList = db.Study_Resource.ToList().FindAll(p => p.Theme_ID == 4);
                    return View(resourceList);
                }// twin commanche
                else
                {
                    return View(resourceList);
                }
            }// if user is not a student
        }// theme screen

        [HttpGet]
        public FileResult downloadResource(int? id)
        {
            Study_Resource downloadResource = db.Study_Resource.Find(id);
            var file = downloadResource.Resources;
            return File(file, "application/pdf");
        }// download file

        public ActionResult addResource(int? id, int? themeId)
        {
            ViewData["themeID"] = "" + themeId;
            Sys_User user = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
            return View(user);
        }// add resource screen

        [HttpPost]
        public ActionResult addResource(int? id, int? themeId, string name, HttpPostedFileBase resource)
        {
            List<Study_Resource> resourceList = db.Study_Resource.ToList();
            if(name == "" || resource == null)
            {
                return RedirectToAction("addResource", new { id = id, themeId = themeId });
            }// if fields are empty
            else
            {
                int resourceId = resourceList.Count + 2;
                Study_Resource newResource = new Study_Resource();
                newResource.Study_Resource_ID = resourceId;
                newResource.Resource_Name = name;
                int theme = Convert.ToInt32(themeId);
                newResource.Theme_ID = theme;

                Stream str = resource.InputStream;
                BinaryReader br = new BinaryReader(str);
                Byte[] fileDetails = br.ReadBytes((Int32)str.Length);
                newResource.Resources = fileDetails;

                db.Study_Resource.Add(newResource);
                db.SaveChanges();

                return RedirectToAction("resourceScreen", new { id = id });
            }// else
        }// add resource post

        public ActionResult deleteResource(int? loggedId, int? id)
        {
            ViewData["loggedId"] = "" + loggedId;
            Study_Resource delResource = db.Study_Resource.ToList().Find(p => p.Study_Resource_ID == id);
            return View(delResource);
        }// delete Resource

        public ActionResult deleteResourceConformation(int? loggedId, int? id)
        {
            Study_Resource delResource = db.Study_Resource.Find(id);
            db.Study_Resource.Remove(delResource);
            db.SaveChanges();
            Student_Resource delStuResource = db.Student_Resource.ToList().Find(r => r.Study_Resource_ID == delResource.Study_Resource_ID);
            db.Student_Resource.Remove(delStuResource);
            db.SaveChanges();
            return RedirectToAction("resourceScreen", new { id = loggedId });
        }// delete conformation

        // GET: Study_Resource/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Study_Resource study_Resource = db.Study_Resource.Find(id);
            if (study_Resource == null)
            {
                return HttpNotFound();
            }
            return View(study_Resource);
        }

        // GET: Study_Resource/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Study_Resource/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Study_Resource_ID,Resource_Name,Resources,Theme_ID")] Study_Resource study_Resource)
        {
            if (ModelState.IsValid)
            {
                db.Study_Resource.Add(study_Resource);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(study_Resource);
        }

        // GET: Study_Resource/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Study_Resource study_Resource = db.Study_Resource.Find(id);
            if (study_Resource == null)
            {
                return HttpNotFound();
            }
            return View(study_Resource);
        }

        // POST: Study_Resource/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Study_Resource_ID,Resource_Name,Resources,Theme_ID")] Study_Resource study_Resource)
        {
            if (ModelState.IsValid)
            {
                db.Entry(study_Resource).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(study_Resource);
        }

        // GET: Study_Resource/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Study_Resource study_Resource = db.Study_Resource.Find(id);
            if (study_Resource == null)
            {
                return HttpNotFound();
            }
            return View(study_Resource);
        }

        // POST: Study_Resource/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Study_Resource study_Resource = db.Study_Resource.Find(id);
            db.Study_Resource.Remove(study_Resource);
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
