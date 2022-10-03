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
    public class QuestionsController : Controller
    {
        private SkyExamsEntities db = new SkyExamsEntities();

        // GET: Questions
        public ActionResult Index()
        {
            return View(db.Questions.ToList());
        }

        public ActionResult exams(int? id)
        {
            try
            {
                if (id != null)
                {
                    ViewData["userID"] = "" + id;
                    Sys_User forRole = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
                    ViewData["userRole"] = "" + forRole.User_Role_ID;
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    List<Plane_Type> planeTypes = db.Plane_Type.ToList();

                    if (forRole.User_Role_ID == 1)
                    {
                        Student tempStu = db.Students.ToList().Find(s => s.SysUser_ID == id);
                        List<Student_Exam> sExam = db.Student_Exam.ToList().FindAll(s => s.Student_ID == tempStu.Student_ID);
                        List<Plane_Type> tempPlaneList = new List<Plane_Type>();
                        foreach (var e in sExam)
                        {
                            Plane_Type temp = db.Plane_Type.ToList().Find(p => p.Plane_Type_ID == e.Exam_ID);
                            tempPlaneList.Add(temp);
                        }// for each
                        planeTypes = tempPlaneList;
                    }// stu check
                    else
                    {
                        planeTypes = db.Plane_Type.ToList();
                    }// else     

                    return View(planeTypes);
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
            
        }// exam screen

        public FileContentResult getImg(int id)
        {
            byte[] byteArray = db.Plane_Type.Find(id).Plane_Image;
            return byteArray != null
                ? new FileContentResult(byteArray, "image/jpeg")
                : null;
        }// get plane images

        public ActionResult examScreen(int? id, int? ratingId)
        {
            try
            {
                if (id != null || ratingId != null)
                {
                    List<Student_Exam> stuExam = db.Student_Exam.ToList().FindAll(e => e.Exam_ID == ratingId);
                    int stuId = db.Students.ToList().Find(s => s.SysUser_ID == id).Student_ID;
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    foreach (var se in stuExam)
                    {
                        if (stuId == se.Student_ID)
                        {
                            ViewData["userId"] = "" + id;
                            ViewData["ratingId"] = "" + ratingId;
                            return View();
                        }// if statement
                    }// foreach
                    return RedirectToAction("exams", new { id = id });
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
            
        }

        public ActionResult studentCheck(int? id, int? ratingId)
        {
            Sys_User temp = db.Sys_User.ToList().Find(u => u.SysUser_ID == id);
            int role = (int)temp.User_Role_ID;
            if (role == 1)
            {
                return RedirectToAction("examScreen", new { id = id, ratingId = ratingId });
            }
            else
            {
                return RedirectToAction("questionsScreen", new { id = id, ratingId = ratingId });
            }
        }// student check

        public ActionResult questionsScreen(int? id, int? ratingId, string message)
        {
            try
            {
                if (id != null || ratingId != null)
                {
                    ViewData["userId"] = "" + id;
                    ViewData["ratingId"] = "" + ratingId;
                    ViewData["Exam"] = db.Plane_Type.ToList().Find(p => p.Plane_Type_ID == ratingId).Type_Description;
                    ViewData["message"] = message;
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    List<Question> questionList = db.Questions.ToList().FindAll(q => q.Question_Rating_ID == ratingId);
                    return View(questionList);
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
            
        }// questions screen

        public ActionResult answersScreen(int? id, int? questionId)
        {
            try
            {
                if (id != null || questionId != null)
                {
                    ViewData["userId"] = "" + id;
                    ViewData["ratingId"] = db.Questions.ToList().Find(q => q.Question_ID == questionId).Question_Rating_ID;
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    ViewData["question"] = db.Questions.ToList().Find(q => q.Question_ID == questionId).Question1;
                    List<Answer> answerList = db.Answers.ToList().FindAll(a => a.Question_ID == questionId);
                    return View(answerList);
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
            
        }// answer screen

        [HttpGet]
        public ActionResult createQuestion(int? id, int? ratingId, string err)
        {
            try
            {
                if (id != null || ratingId != null)
                {
                    ViewData["userId"] = "" + id;
                    ViewData["ratingId"] = "" + ratingId;
                    ViewData["err"] = err;
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    return View(db.Sections.ToList());
                }
                else
                {
                    return RedirectToAction("loginScreen", "Sys_User");
                }
            }
            catch(NullReferenceException e)
            {
                ViewData["err"] = e;
                return RedirectToAction("loginScreen", "Sys_User");
            }
            
        }// create question get

        [HttpPost]
        public ActionResult createQuestion(int? id, int? ratingId, int sec, string question, string correct, string false1, string false2)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    if (question == "" || correct == "" || false1 == "" || false2 == "")
                    {
                        string temp = "Hint: Complete all the fields before clicking submit.";
                        return RedirectToAction("createQuestion", new { id = id, ratingId = ratingId, err = temp });
                    }// if statement
                    else
                    {
                        //int qID = db.Questions.ToList().Count + 2;
                        //int aID = db.Answers.ToList().Count + 2;
                        Question newQ = new Question();
                        //newQ.Question_ID = qID;
                        newQ.Question1 = question;
                        newQ.Section_ID = sec;
                        newQ.Question_Rating_ID = Convert.ToInt32(ratingId);
                        db.Questions.Add(newQ);
                        db.SaveChanges();

                        Answer correctAns = new Answer();
                        //correctAns.Answer_ID = aID;
                        correctAns.ANS = correct;
                        correctAns.Question_ID = newQ.Question_ID;
                        correctAns.Correct_Answer = true;
                        db.Answers.Add(correctAns);
                        db.SaveChanges();

                        Answer falseAns1 = new Answer();
                        //falseAns1.Answer_ID = aID + 1;
                        falseAns1.ANS = false1;
                        falseAns1.Question_ID = newQ.Question_ID;
                        falseAns1.Correct_Answer = false;
                        db.Answers.Add(falseAns1);
                        db.SaveChanges();

                        Answer falseAns2 = new Answer();
                        //falseAns2.Answer_ID = aID + 2;
                        falseAns2.ANS = false2;
                        falseAns2.Question_ID = newQ.Question_ID;
                        falseAns2.Correct_Answer = false;
                        db.Answers.Add(falseAns2);
                        db.SaveChanges();

                        return RedirectToAction("questionsScreen", new { id = id, ratingId = ratingId });
                    }// else
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
            
        }// create question post

        [HttpGet]
        public ActionResult addLoadSheet(int? id, int? ratingId, string err)
        {
            try
            {
                if (id != null || ratingId != null)
                {
                    ViewData["ratingId"] = "" + ratingId;
                    ViewData["err"] = err;
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    Sys_User tempUser = db.Sys_User.ToList().Find(s => s.SysUser_ID == id);
                    return View(tempUser);
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
            
        }// add load sheet get

        [HttpPost]
        public ActionResult addLoadSheet(int? id, int? ratingId, HttpPostedFileBase loadSheet)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    if (loadSheet == null)
                    {
                        string temp = "Hint: Complete all the fields before clicking submit.";
                        return RedirectToAction("addLoadSheet", new { id = id, ratingId = ratingId, err = temp });
                    }// if fields are empty
                    else
                    {
                        Load_Sheet tempLS = db.Load_Sheet.ToList().Find(l => l.Exam_ID == Convert.ToInt32(ratingId));
                        if (tempLS == null)
                        {
                            Load_Sheet newLS = new Load_Sheet();
                            newLS.Exam_ID = Convert.ToInt32(ratingId);

                            Stream str = loadSheet.InputStream;
                            BinaryReader br = new BinaryReader(str);
                            Byte[] fileDetails = br.ReadBytes((Int32)str.Length);
                            newLS.load_Sheet1 = fileDetails;

                            string ext = Path.GetExtension(loadSheet.FileName).ToUpper();
                            if (ext == ".PDF")
                            {
                                db.Load_Sheet.Add(newLS);
                                db.SaveChanges();
                            }
                            else
                            {
                                string temp = "Hint: Upload a PDF.";
                                return RedirectToAction("addLoadSheet", new { id = id, ratingId = ratingId, err = temp });
                            }

                        }// if no load sheet exists
                        else
                        {
                            db.Load_Sheet.Remove(tempLS);
                            db.SaveChanges();

                            Load_Sheet newLS = new Load_Sheet();
                            newLS.Exam_ID = Convert.ToInt32(ratingId);

                            Stream str = loadSheet.InputStream;
                            BinaryReader br = new BinaryReader(str);
                            Byte[] fileDetails = br.ReadBytes((Int32)str.Length);
                            newLS.load_Sheet1 = fileDetails;

                            string ext = Path.GetExtension(loadSheet.FileName).ToUpper();
                            if (ext == ".PDF")
                            {
                                db.Load_Sheet.Add(newLS);
                                db.SaveChanges();
                            }
                            else
                            {
                                string temp = "Hint: Upload a PDF.";
                                return RedirectToAction("addLoadSheet", new { id = id, ratingId = ratingId, err = temp });
                            }
                        }// if load sheet exists
                    }// else

                    return RedirectToAction("questionsScreen", new { id = id, ratingId = ratingId });
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
            
        }// add load sheet post

        public FileResult downloadLoadSheet(int? id)
        {
            Load_Sheet downloadLoadSheet = db.Load_Sheet.ToList().Find(l => l.Exam_ID == id);
            if(downloadLoadSheet != null)
            {
                var file = downloadLoadSheet.load_Sheet1;
                return File(file, "application/pdf");
            }// if loadsheet exists
            else
            {
                return null;
            }// no load sheet
        }// download file

        public ActionResult deleteLoadSheet(int? id, int? ratingId)
        {
            try
            {
                if (id != null || ratingId != null)
                {
                    Load_Sheet delSheet = db.Load_Sheet.ToList().Find(l => l.Exam_ID == ratingId);
                    string temp = "No loadsheet found.";
                    if(delSheet != null)
                    {
                        db.Load_Sheet.Remove(delSheet);
                        db.SaveChanges();
                        temp = "Loadsheet deleted successfully.";
                    }// if statement
                    return RedirectToAction("questionsScreen", new { id = id, ratingId = ratingId, message = temp });
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
            
        }// delete load sheet

        [HttpGet]
        public ActionResult deleteQuestion(int? id, int? questionId)
        {
            try
            {
                if (id != null || questionId != null)
                {
                    ViewData["userId"] = "" + id;
                    ViewData["ratingId"] = db.Questions.ToList().Find(q => q.Question_ID == questionId).Question_Rating_ID;
                    ViewData["questionId"] = "" + questionId;
                    ViewData["question"] = db.Questions.ToList().Find(q => q.Question_ID == questionId).Question1;
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    return View(db.Answers.ToList().FindAll(a => a.Question_ID == questionId));
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
            
        }// delete question get

        public ActionResult deleteQuestionConformation(int? id, int? questionId)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    Question delQuest = db.Questions.ToList().Find(q => q.Question_ID == questionId);
                    db.Questions.Remove(delQuest);
                    db.SaveChanges();
                    List<Answer> delAnsList = db.Answers.ToList().FindAll(a => a.Question_ID == questionId);
                    foreach (var ans in delAnsList)
                    {
                        db.Answers.Remove(ans);
                        db.SaveChanges();
                    }// for each
                    return RedirectToAction("questionsScreen", new { id = id, ratingId = delQuest.Question_Rating_ID });
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
            
        }// delete question conformation

        [HttpGet]
        public ActionResult updateQuestion(int? id, int? questionId, string err)
        {
            try
            {
                if (id != null || questionId != null)
                {
                    ViewData["userId"] = "" + id;
                    ViewData["questionId"] = "" + questionId;
                    ViewData["ratingId"] = db.Questions.ToList().Find(q => q.Question_ID == questionId).Question_Rating_ID;
                    ViewData["question"] = db.Questions.ToList().Find(q => q.Question_ID == questionId).Question1;
                    List<Answer> updateAnsList = db.Answers.ToList().FindAll(a => a.Question_ID == questionId && a.Correct_Answer == false);
                    Answer correctAns = db.Answers.ToList().Find(a => a.Question_ID == questionId && a.Correct_Answer == true);
                    ViewData["correctAnsId"] = correctAns.Answer_ID;
                    ViewData["correctAns"] = correctAns.ANS;
                    ViewData["false1Id"] = db.Answers.ToList().Find(a => a.Answer_ID == correctAns.Answer_ID + 1).Answer_ID;
                    ViewData["false1"] = db.Answers.ToList().Find(a => a.Answer_ID == correctAns.Answer_ID + 1).ANS;
                    ViewData["false2Id"] = db.Answers.ToList().Find(a => a.Answer_ID == correctAns.Answer_ID + 2).Answer_ID;
                    ViewData["false2"] = db.Answers.ToList().Find(a => a.Answer_ID == correctAns.Answer_ID + 2).ANS;
                    ViewData["err"] = err;
                    ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                    return View(db.Sections.ToList());
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
            
        }// update question get

        [HttpPost]
        public ActionResult updateQuestion(int? id,  int? questionId, int? ratingId, int? correctAnsId, int? false1Id, int? false2Id, int sec, string question, string correct, string false1, string false2)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    if (question == "" || correct == "" || false1 == "" || false2 == "")
                    {
                        string temp = "Hint: Complete all the fields before clicking submit.";
                        return RedirectToAction("updateQuestion", new { id = id, ratingId = ratingId, err = temp });
                    }// if statement
                    else
                    {
                        Question oldQuestion = db.Questions.ToList().Find(q => q.Question_ID == questionId);
                        Question newQuestion = new Question();
                        //newQuestion.Question_ID = oldQuestion.Question_ID;
                        newQuestion.Question1 = question;
                        newQuestion.Section_ID = sec;
                        newQuestion.Question_Rating_ID = Convert.ToInt32(ratingId);
                        db.Questions.Remove(oldQuestion);
                        db.SaveChanges();
                        db.Questions.Add(newQuestion);
                        db.SaveChanges();

                        Answer oldCorrect = db.Answers.ToList().Find(a => a.Answer_ID == correctAnsId);
                        Answer newCorrect = new Answer();
                        //newCorrect.Answer_ID = oldCorrect.Answer_ID;
                        newCorrect.ANS = correct;
                        newCorrect.Question_ID = newQuestion.Question_ID;
                        newCorrect.Correct_Answer = true;
                        db.Answers.Remove(oldCorrect);
                        db.SaveChanges();
                        db.Answers.Add(newCorrect);
                        db.SaveChanges();

                        Answer oldFalse1 = db.Answers.ToList().Find(a => a.Answer_ID == false1Id);
                        Answer newFalse1 = new Answer();
                        //newFalse1.Answer_ID = oldFalse1.Answer_ID;
                        newFalse1.ANS = false1;
                        newFalse1.Question_ID = newQuestion.Question_ID;
                        newFalse1.Correct_Answer = false;
                        db.Answers.Remove(oldFalse1);
                        db.SaveChanges();
                        db.Answers.Add(newFalse1);
                        db.SaveChanges();

                        Answer oldFalse2 = db.Answers.ToList().Find(a => a.Answer_ID == false2Id);
                        Answer newFalse2 = new Answer();
                        //newFalse2.Answer_ID = oldFalse2.Answer_ID;
                        newFalse2.ANS = false2;
                        newFalse2.Question_ID = newQuestion.Question_ID;
                        newFalse2.Correct_Answer = false;
                        db.Answers.Remove(oldFalse2);
                        db.SaveChanges();
                        db.Answers.Add(newFalse2);
                        db.SaveChanges();
                    }

                    return RedirectToAction("questionsScreen", new { id = id, ratingId = ratingId });
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
            
        }// update question post

        [HttpGet]
        public ActionResult writeExam(int? id, int? ratingId)
        {
            try
            {
                if (id != null || ratingId != null)
                {
                    Student_Exam stuExam = db.Student_Exam.ToList().Find(s => s.Student_ID == id && s.Exam_ID == ratingId);
                    if (stuExam.Started == true && stuExam.Completed == false)
                    {
                        ViewData["userId"] = "" + id;
                        ViewData["ratingId"] = "" + ratingId;
                        ViewData["time"] = db.Timers.ToList().Find(t => t.Timer_ID == 1).Timer_Value * 60000;
                        Sys_User tempUser = db.Sys_User.ToList().Find(s => s.SysUser_ID == id);
                        ViewData["userName"] = tempUser.FName + " " + tempUser.Surname;
                        List<QuestionVM> questions = new List<QuestionVM>();
                        List<Question> tempQ = db.Questions.ToList().FindAll(q => q.Question_Rating_ID == ratingId);

                        List<Save_Exam> savedQuestions = db.Save_Exam.ToList().FindAll(s => s.Student_Exam_ID == stuExam.Stu_Exam);

                        foreach (var s in savedQuestions)
                        {
                            QuestionVM tempQuestion = new QuestionVM();
                            tempQuestion.QuestionID = s.QuestionID;
                            tempQuestion.QuestionText = db.Questions.ToList().Find(q => q.Question_ID == s.QuestionID).Question1;
                            tempQuestion.Answers = db.Answers.ToList().FindAll(a => a.Answer_ID == s.AnswerID);
                            questions.Add(tempQuestion);
                        }// for each

                        if (savedQuestions.Count != tempQ.Count)
                        {
                            Random rng = new Random();
                            int n = tempQ.Count;
                            while (n > 1)
                            {
                                n--;
                                int k = rng.Next(n + 1);
                                Question value = tempQ[k];
                                tempQ[k] = tempQ[n];
                                tempQ[n] = value;
                            }// shuffle while
                            foreach (var q in tempQ)
                            {
                                QuestionVM temp = new QuestionVM();
                                temp.QuestionID = q.Question_ID;
                                temp.QuestionText = q.Question1;
                                List<Answer> tempAns = db.Answers.ToList().FindAll(a => a.Question_ID == q.Question_ID);
                                Random rng2 = new Random();
                                int n2 = tempAns.Count - 1;
                                while (n2 > 1)
                                {
                                    n2--;
                                    int k2 = rng.Next(n2 + 1);
                                    Answer value = tempAns[k2];
                                    tempAns[k2] = tempAns[n2];
                                    tempAns[n2] = value;
                                }// shuffle while
                                temp.Answers = tempAns;

                                QuestionVM questionCheck = questions.Find(t => t.QuestionID == q.Question_ID);
                                if (questionCheck == null)
                                {
                                    questions.Add(temp);
                                }// question check

                            }// for each
                        }// if statement

                        if (questions.Count() > tempQ.Count())
                        {
                            questions.RemoveRange(tempQ.Count(), (questions.Count - tempQ.Count()));
                        }// change questions array length

                        if (questions.Count() > 100)
                        {
                            questions.RemoveRange(100, questions.Count());
                        }// 100 questions
                        return View(questions);
                    }// if student has started the exam
                    else
                    {
                        Student_Exam newStuExam = new Student_Exam();
                        newStuExam.Student_ID = stuExam.Student_ID;
                        newStuExam.Exam_ID = stuExam.Exam_ID;
                        newStuExam.Exam_Mark = 0;
                        newStuExam.Started = true;
                        newStuExam.Completed = false;

                        db.Student_Exam.Remove(stuExam);
                        db.SaveChanges();

                        db.Student_Exam.Add(newStuExam);
                        db.SaveChanges();

                        ViewData["userId"] = "" + id;
                        ViewData["ratingId"] = "" + ratingId;
                        Sys_User tempUser = db.Sys_User.ToList().Find(s => s.SysUser_ID == id);
                        ViewData["userName"] = tempUser.FName + " " + tempUser.Surname;
                        List<QuestionVM> questions = new List<QuestionVM>();
                        List<Question> tempQ = db.Questions.ToList().FindAll(q => q.Question_Rating_ID == ratingId);
                        Random rng = new Random();
                        int n = tempQ.Count;
                        while (n > 1)
                        {
                            n--;
                            int k = rng.Next(n + 1);
                            Question value = tempQ[k];
                            tempQ[k] = tempQ[n];
                            tempQ[n] = value;
                        }// shuffle while
                        foreach (var q in tempQ)
                        {
                            QuestionVM temp = new QuestionVM();
                            temp.QuestionID = q.Question_ID;
                            temp.QuestionText = q.Question1;
                            List<Answer> tempAns = db.Answers.ToList().FindAll(a => a.Question_ID == q.Question_ID);
                            Random rng2 = new Random();
                            int n2 = tempAns.Count - 1;
                            while (n2 > 1)
                            {
                                n2--;
                                int k2 = rng.Next(n2 + 1);
                                Answer value = tempAns[k2];
                                tempAns[k2] = tempAns[n2];
                                tempAns[n2] = value;
                            }// shuffle while
                            temp.Answers = tempAns;
                            questions.Add(temp);
                        }// for each

                        if (questions.Count() > 100)
                        {
                            questions.RemoveRange(100, questions.Count());
                        }// 100 questions

                        return View(questions);
                    }// if student hasent started the exam
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
            
            
        }// write exam get

        [HttpPost]
        public ActionResult writeExam(int? id, int? ratingId, List<AnswerVM> resultExam)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    List<AnswerVM> finalResultQuiz = new List<AnswerVM>();
                    double correctAns = 0;
                    double totAns = 0;
                    double grade = 0;
                    foreach (AnswerVM answser in resultExam)
                    {
                        AnswerVM result = db.Answers.Where(a => a.Question_ID == answser.QuestionID).Select(a => new AnswerVM
                        {
                            QuestionID = a.Question_ID,
                            AnswerID = a.Answer_ID,
                            AnswerQ = a.ANS,
                            isCorrect = (answser.AnswerID == a.Answer_ID)

                        }).FirstOrDefault();
                        if (result.isCorrect == true)
                        {
                            correctAns = correctAns + 1;
                        }// if statement
                        totAns = totAns + 1;

                        finalResultQuiz.Add(result);
                    }

                    grade = (correctAns / totAns) * 100;
                    int finalGrade = Convert.ToInt32(grade);
                    int stuID = db.Students.ToList().Find(s => s.SysUser_ID == id).Student_ID;

                    Student_Exam tempStuExam = db.Student_Exam.ToList().Find(e => e.Student_ID == stuID && e.Exam_ID == ratingId);
                    List<Save_Exam> savedQuestions = db.Save_Exam.ToList().FindAll(s => s.Student_Exam_ID == tempStuExam.Stu_Exam);
                    Student_Exam newStuExam = new Student_Exam();
                    newStuExam.Student_ID = tempStuExam.Student_ID;
                    newStuExam.Exam_ID = tempStuExam.Exam_ID;
                    newStuExam.Started = true;
                    newStuExam.Completed = true;
                    newStuExam.Exam_Mark = finalGrade;
                    newStuExam.Date_Completed = DateTime.Now;

                    foreach (var s in savedQuestions)
                    {
                        db.Save_Exam.Remove(s);
                        db.SaveChanges();
                    }// for each

                    db.Student_Exam.Remove(tempStuExam);
                    db.SaveChanges();

                    newStuExam.Stu_Exam = tempStuExam.Stu_Exam;
                    db.Student_Exam.Add(newStuExam);
                    db.SaveChanges();

                    List<Student_Exam> stuExams = db.Student_Exam.ToList().FindAll(s => s.Exam_ID == ratingId && s.Completed == true);
                    int totalMarks = 0;
                    foreach (var e in stuExams)
                    {
                        totalMarks = totalMarks + Convert.ToInt32(e.Exam_Mark);
                    }// for each

                    int totalExams = stuExams.Count();
                    int average = totalMarks / totalExams;

                    Exam_Average newAverage = new Exam_Average();
                    newAverage.Exam_ID = Convert.ToInt32(ratingId);
                    newAverage.Average = average;

                    Exam_Average tempAverage = db.Exam_Average.ToList().Find(e => e.Exam_ID == ratingId);
                    if (tempAverage != null)
                    {
                        db.Exam_Average.Remove(tempAverage);
                        db.SaveChanges();
                    }// if statement

                    db.Exam_Average.Add(newAverage);
                    db.SaveChanges();

                    Sys_User tempUser = db.Sys_User.ToList().Find(s => s.SysUser_ID == Convert.ToInt32(id));
                    Student tempStu = db.Students.ToList().Find(s => s.SysUser_ID == tempUser.SysUser_ID);

                    Registration_Sheet newSheet = new Registration_Sheet();
                    newSheet.Sys_User_ID = (int)id;
                    newSheet.First_Name = tempUser.FName;
                    newSheet.Surname = tempUser.Surname;
                    newSheet.Licence_No = tempStu.Licence_No;
                    newSheet.Plane_Type_ID = (int)ratingId;
                    newSheet.Date_Written = DateTime.Now;
                    newSheet.Type_Desctription = db.Plane_Type.ToList().Find(p => p.Plane_Type_ID == Convert.ToInt32(ratingId)).Type_Description;
                    newSheet.Mark = finalGrade;

                    db.Registration_Sheet.Add(newSheet);
                    db.SaveChanges();

                    return Json(new { result = finalResultQuiz }, JsonRequestBehavior.AllowGet);
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
            
        }// write exam post

        [HttpPost]
        public ActionResult saveExam(int? id, int? ratingId, List<AnswerVM> examSave)
        {
            try
            {
                if (Request.Cookies["AuthID"].Value == Session["AuthID"].ToString())
                {
                    List<Save_Exam> sExam = db.Save_Exam.ToList();
                    int count = 0;
                    Student_Exam stuExam = db.Student_Exam.ToList().Find(s => s.Student_ID == id && s.Exam_ID == ratingId);
                    if (examSave != null)
                    {
                        foreach (AnswerVM ans in examSave)
                        {

                            try
                            {
                                Save_Exam tempSave = new Save_Exam();
                                //tempSave.Save_Exam_ID = sExam[count].Save_Exam_ID + 1;
                                tempSave.QuestionID = ans.QuestionID;
                                tempSave.AnswerID = ans.AnswerID;
                                tempSave.Student_Exam_ID = stuExam.Stu_Exam;
                                if (ans.AnswerID != sExam[count].AnswerID && sExam[count].Student_Exam_ID != stuExam.Stu_Exam)
                                {
                                    db.Save_Exam.Add(tempSave);
                                    db.SaveChanges();
                                }
                            }// try
                            catch
                            {
                                Save_Exam tempSave = new Save_Exam();
                                //tempSave.Save_Exam_ID = sExam.Count + 3;
                                tempSave.QuestionID = ans.QuestionID;
                                tempSave.AnswerID = ans.AnswerID;
                                tempSave.Student_Exam_ID = stuExam.Stu_Exam;
                                db.Save_Exam.Add(tempSave);
                                db.SaveChanges();
                            }// catch
                            count++;
                        }// for each
                    }// if statement
                    else
                    {
                        Student_Exam newSExam = new Student_Exam();
                        newSExam.Student_ID = stuExam.Student_ID;
                        newSExam.Exam_ID = stuExam.Exam_ID;
                        newSExam.Exam_Mark = 0;
                        newSExam.Started = false;
                        newSExam.Completed = false;

                        db.Student_Exam.Remove(stuExam);
                        db.SaveChanges();

                        db.Student_Exam.Add(newSExam);
                        db.SaveChanges();
                    }// else statement

                    return RedirectToAction("exams", new { id = id });
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

        // GET: Questions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Question question = db.Questions.Find(id);
            if (question == null)
            {
                return HttpNotFound();
            }
            return View(question);
        }

        // GET: Questions/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Questions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Question_ID,Question1,Section_ID,Question_Rating_ID")] Question question)
        {
            if (ModelState.IsValid)
            {
                db.Questions.Add(question);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(question);
        }

        // GET: Questions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Question question = db.Questions.Find(id);
            if (question == null)
            {
                return HttpNotFound();
            }
            return View(question);
        }

        // POST: Questions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Question_ID,Question1,Section_ID,Question_Rating_ID")] Question question)
        {
            if (ModelState.IsValid)
            {
                db.Entry(question).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(question);
        }

        // GET: Questions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Question question = db.Questions.Find(id);
            if (question == null)
            {
                return HttpNotFound();
            }
            return View(question);
        }

        // POST: Questions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Question question = db.Questions.Find(id);
            db.Questions.Remove(question);
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
