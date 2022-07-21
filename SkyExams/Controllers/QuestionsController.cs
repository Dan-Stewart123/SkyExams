using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
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
            return View(db.Sys_User.ToList().Find(u => u.SysUser_ID == id));
        }// exam

        public ActionResult examScreen(int? id, int? ratingId)
        {
            List<Student_Exam> stuExam = db.Student_Exam.ToList().FindAll(e => e.Exam_ID == ratingId);
            int stuId = db.Students.ToList().Find(s => s.SysUser_ID == id).Student_ID;
            foreach(var se in stuExam)
            {
                if(stuId == se.Student_ID)
                {
                    ViewData["userId"] = "" + id;
                    ViewData["ratingId"] = "" + ratingId;
                    return View();
                }// if statement
            }// foreach
            return RedirectToAction("exams", new { id = id });
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

        public ActionResult questionsScreen(int? id, int? ratingId)
        {
            ViewData["userId"] = "" + id;
            ViewData["ratingId"] = "" + ratingId;
            if(ratingId == 1)
            {
                ViewData["Exam"] = "Cessna 172";
            }
            if (ratingId == 2)
            {
                ViewData["Exam"] = "Cessna 172 RG";
            }
            if (ratingId == 3)
            {
                ViewData["Exam"] = "Chrokee 140";
            }
            if (ratingId == 4)
            {
                ViewData["Exam"] = "Twin Commanche";
            }
            List<Question> questionList = db.Questions.ToList().FindAll(q => q.Question_Rating_ID == ratingId);
            return View(questionList);
        }// questions screen

        public ActionResult answersScreen(int? id, int? questionId)
        {
            ViewData["userId"] = "" + id;
            ViewData["ratingId"] = db.Questions.ToList().Find(q => q.Question_ID == questionId).Question_Rating_ID;
            ViewData["question"] = db.Questions.ToList().Find(q => q.Question_ID == questionId).Question1;
            List<Answer> answerList = db.Answers.ToList().FindAll(a => a.Question_ID == questionId);
            return View(answerList);
        }// answer screen

        [HttpGet]
        public ActionResult createQuestion(int? id, int? ratingId)
        {
            ViewData["userId"] = "" + id;
            ViewData["ratingId"] = "" + ratingId;
            return View(db.Sections.ToList());
        }// create question get

        [HttpPost]
        public ActionResult createQuestion(int? id, int? ratingId, int sec, string question, string correct, string false1, string false2)
        {
            if(question == null || correct == null || false1 == null || false2 == null)
            {
                return View();
            }// if statement
            else
            {
                int qID = db.Questions.ToList().Count + 2;
                int aID = db.Answers.ToList().Count + 2;
                Question newQ = new Question();
                newQ.Question_ID = qID;
                newQ.Question1 = question;
                newQ.Section_ID = sec;
                newQ.Question_Rating_ID = Convert.ToInt32(ratingId);
                db.Questions.Add(newQ);
                db.SaveChanges();

                Answer correctAns = new Answer();
                correctAns.Answer_ID = aID;
                correctAns.ANS = correct;
                correctAns.Question_ID = newQ.Question_ID;
                correctAns.Correct_Answer = true;
                db.Answers.Add(correctAns);
                db.SaveChanges();

                Answer falseAns1 = new Answer();
                falseAns1.Answer_ID = aID + 1;
                falseAns1.ANS = false1;
                falseAns1.Question_ID = newQ.Question_ID;
                falseAns1.Correct_Answer = false;
                db.Answers.Add(falseAns1);
                db.SaveChanges();

                Answer falseAns2 = new Answer();
                falseAns2.Answer_ID = aID + 2;
                falseAns2.ANS = false2;
                falseAns2.Question_ID = newQ.Question_ID;
                falseAns2.Correct_Answer = false;
                db.Answers.Add(falseAns2);
                db.SaveChanges();

                return RedirectToAction("questionsScreen", new { id = id, ratingId = ratingId });
            }// else
        }// create question post

        [HttpGet]
        public ActionResult deleteQuestion(int? id, int? questionId)
        {
            ViewData["userId"] = "" + id;
            ViewData["ratingId"] = db.Questions.ToList().Find(q => q.Question_ID == questionId).Question_Rating_ID;
            ViewData["questionId"] = "" + questionId;
            ViewData["question"] = db.Questions.ToList().Find(q => q.Question_ID == questionId).Question1;
            return View(db.Answers.ToList().FindAll(a => a.Question_ID == questionId));
        }// delete question get

        public ActionResult deleteQuestionConformation(int? id, int? questionId)
        {
            Question delQuest = db.Questions.ToList().Find(q => q.Question_ID == questionId);
            db.Questions.Remove(delQuest);
            db.SaveChanges();
            List<Answer> delAnsList = db.Answers.ToList().FindAll(a => a.Question_ID == questionId);
            foreach(var ans in delAnsList)
            {
                db.Answers.Remove(ans);
                db.SaveChanges();
            }// for each
            return RedirectToAction("questionsScreen", new { id = id, ratingId = delQuest.Question_Rating_ID });
        }// delete question conformation

        [HttpGet]
        public ActionResult updateQuestion(int? id, int? questionId)
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
            return View(db.Sections.ToList());
        }// update question get

        [HttpPost]
        public ActionResult updateQuestion(int? id,  int? questionId, int? ratingId, int? correctAnsId, int? false1Id, int? false2Id, int sec, string question, string correct, string false1, string false2)
        {
            Question oldQuestion = db.Questions.ToList().Find(q => q.Question_ID == questionId);
            Question newQuestion = new Question();
            newQuestion.Question_ID = oldQuestion.Question_ID;
            newQuestion.Question1 = question;
            newQuestion.Section_ID = sec;
            newQuestion.Question_Rating_ID = Convert.ToInt32(ratingId);
            db.Questions.Remove(oldQuestion);
            db.SaveChanges();
            db.Questions.Add(newQuestion);
            db.SaveChanges();

            Answer oldCorrect = db.Answers.ToList().Find(a => a.Answer_ID == correctAnsId);
            Answer newCorrect = new Answer();
            newCorrect.Answer_ID = oldCorrect.Answer_ID;
            newCorrect.ANS = correct;
            newCorrect.Question_ID = newQuestion.Question_ID;
            newCorrect.Correct_Answer = true;
            db.Answers.Remove(oldCorrect);
            db.SaveChanges();
            db.Answers.Add(newCorrect);
            db.SaveChanges();

            Answer oldFalse1 = db.Answers.ToList().Find(a => a.Answer_ID == false1Id);
            Answer newFalse1 = new Answer();
            newFalse1.Answer_ID = oldFalse1.Answer_ID;
            newFalse1.ANS = false1;
            newFalse1.Question_ID = newQuestion.Question_ID;
            newFalse1.Correct_Answer = false;
            db.Answers.Remove(oldFalse1);
            db.SaveChanges();
            db.Answers.Add(newFalse1);
            db.SaveChanges();

            Answer oldFalse2 = db.Answers.ToList().Find(a => a.Answer_ID == false2Id);
            Answer newFalse2 = new Answer();
            newFalse2.Answer_ID = oldFalse2.Answer_ID;
            newFalse2.ANS = false2;
            newFalse2.Question_ID = newQuestion.Question_ID;
            newFalse2.Correct_Answer = false;
            db.Answers.Remove(oldFalse2);
            db.SaveChanges();
            db.Answers.Add(newFalse2);
            db.SaveChanges();

            return RedirectToAction("questionsScreen", new { id = id, ratingId = ratingId });
        }// update question post

        [HttpGet]
        public ActionResult writeExam(int? id, int? ratingId)
        {
            ViewData["userId"] = "" + id;
            ViewData["ratingId"] = "" + ratingId;
            List <QuestionVM> questions = new List<QuestionVM>();
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
            foreach(var q in tempQ)
            {
                QuestionVM temp = new QuestionVM();
                temp.QuestionID = q.Question_ID;
                temp.QuestionText = q.Question1;
                List<Answer> tempAns = db.Answers.ToList().FindAll(a => a.Question_ID == q.Question_ID);
                Random rng2 = new Random();
                int n2 = tempQ.Count;
                while (n > 1)
                {
                    n--;
                    int k = rng.Next(n2 + 1);
                    Answer value = tempAns[k];
                    tempAns[k] = tempAns[n2];
                    tempAns[n2] = value;
                }// shuffle while
                temp.Answers = tempAns;
                questions.Add(temp);
            }// for each
            return View(questions);
        }// write exam get

        [HttpPost]
        public ActionResult writeExam(int? id, int? ratingId, List<AnswerVM> resultExam)
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
                    AnswerQ = a.ANS,
                    isCorrect = (answser.AnswerQ.ToLower().Equals(a.ANS.ToLower()))

                }).FirstOrDefault();
                if(result.isCorrect == true)
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
            Student_Exam newStuExam = new Student_Exam();
            newStuExam.Student_ID = tempStuExam.Student_ID;
            newStuExam.Exam_ID = tempStuExam.Exam_ID;
            newStuExam.Completed = true;
            newStuExam.Exam_Mark = finalGrade;

            db.Student_Exam.Remove(tempStuExam);
            db.SaveChanges();

            newStuExam.Stu_Exam = tempStuExam.Stu_Exam;
            db.Student_Exam.Add(newStuExam);
            db.SaveChanges();

            return Json(new { result = finalResultQuiz }, JsonRequestBehavior.AllowGet);
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
