using AdamsScienceHub.Data;
using AdamsScienceHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdamsScienceHub.Controllers
{
    public class AdminQuestionsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AdminQuestionsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // MAIN MANAGEMENT PAGE - SHOWS SUBJECTS (TEMPORARY FIX)
        public IActionResult ManageQuestions()
        {
            // Temporary fix - remove Include for now
            var subjects = _db.Subjects.ToList();
            return View(subjects);
        }

        // ADD NEW SUBJECT
        // ADD NEW SUBJECT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddSubject(string subjectName, bool calculatorEnabled = false)
        {
            var subject = new Subject
            {
                SubjectName = subjectName,
            };

            _db.Subjects.Add(subject);
            _db.SaveChanges();
            return RedirectToAction("ManageQuestions");
        }


        // VIEW QUESTIONS FOR SPECIFIC SUBJECT
        public IActionResult ViewQuestions(int subjectId)
        {
            var subject = _db.Subjects.Find(subjectId);
            var questions = _db.Questions.Where(q => q.SubjectId == subjectId).ToList();

            ViewBag.Subject = subject;
            return View(questions);
        }

        // DELETE SUBJECT AND ITS QUESTIONS (TEMPORARY FIX)
        public IActionResult DeleteSubject(int id)
        {
            var subject = _db.Subjects.Find(id);
            if (subject != null)
            {
                // Manually delete related questions first
                var questions = _db.Questions.Where(q => q.SubjectId == id).ToList();
                _db.Questions.RemoveRange(questions);

                _db.Subjects.Remove(subject);
                _db.SaveChanges();
            }
            return RedirectToAction("ManageQuestions");
        }

        // ADD QUESTION PAGE
        public async Task<IActionResult> AddQuestion(int subjectId)
        {
            var subject = await _db.Subjects.FindAsync(subjectId);
            if (subject == null) return NotFound();

            ViewBag.Subject = subject;
            ViewBag.Subjects = await _db.Subjects.ToListAsync();

            var question = new Question { SubjectId = subjectId };
            return View(question);
        }

        // ADD QUESTION POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuestion(Question question)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Subjects = await _db.Subjects.ToListAsync();
                ViewBag.Subject = await _db.Subjects.FindAsync(question.SubjectId);
                return View(question);
            }

            question.CreatedAt = DateTime.Now;
            _db.Questions.Add(question);
            await _db.SaveChangesAsync();

            return RedirectToAction("ViewQuestions", new { subjectId = question.SubjectId });
        }

        // EDIT PAGE
        public async Task<IActionResult> EditQuestion(int id)
        {
            var q = await _db.Questions.FindAsync(id);
            if (q == null) return NotFound();

            ViewBag.Subjects = _db.Subjects
                .Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = s.SubjectId.ToString(),
                    Text = s.SubjectName
                })
                .ToList();

            return View(q);
        }

        // EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuestion(Question question)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Subjects = _db.Subjects
                    .Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = s.SubjectId.ToString(),
                        Text = s.SubjectName
                    })
                    .ToList();

                return View(question);
            }

            _db.Questions.Update(question);
            await _db.SaveChangesAsync();
            return RedirectToAction("ViewQuestions", new { subjectId = question.SubjectId });
        }


        // DELETE QUESTION
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var q = await _db.Questions.FindAsync(id);
            if (q != null)
            {
                var subjectId = q.SubjectId;
                _db.Questions.Remove(q);
                await _db.SaveChangesAsync();
                return RedirectToAction("ViewQuestions", new { subjectId = subjectId });
            }

            return RedirectToAction("ManageQuestions");
        }

        // REMOVED DUPLICATE METHODS: Manage() and Create() since we have AddQuestion
    }
}