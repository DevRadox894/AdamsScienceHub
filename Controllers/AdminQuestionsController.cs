using AdamsScienceHub.Data;
using AdamsScienceHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        // MAIN MANAGEMENT PAGE - SHOWS SUBJECTS
        public IActionResult ManageQuestions()
        {
            var subjects = _db.Subjects.ToList();
            return View(subjects);
        }

        // ADD NEW SUBJECT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddSubject(string subjectName)
        {
            if (string.IsNullOrWhiteSpace(subjectName))
            {
                TempData["Error"] = "Subject name cannot be empty.";
                return RedirectToAction("ManageQuestions");
            }

            var subject = new Subject
            {
                SubjectName = subjectName
            };

            _db.Subjects.Add(subject);
            _db.SaveChanges();
            TempData["Success"] = "Subject added successfully!";
            return RedirectToAction("ManageQuestions");
        }

        // VIEW QUESTIONS FOR SPECIFIC SUBJECT
        public IActionResult ViewQuestions(int subjectId)
        {
            var subject = _db.Subjects.Find(subjectId);
            if (subject == null) return NotFound();

            var questions = _db.Questions
                .Where(q => q.SubjectId == subjectId)
                .ToList();

            ViewBag.Subject = subject;
            return View(questions);
        }

        // ADD QUESTION PAGE
        public async Task<IActionResult> AddQuestion(int subjectId)
        {
            var subject = await _db.Subjects.FindAsync(subjectId);
            if (subject == null) return NotFound();

            ViewBag.Subject = subject;
            ViewBag.Subjects = new SelectList(await _db.Subjects.ToListAsync(), "SubjectId", "SubjectName");

            var question = new Question { SubjectId = subjectId };
            return View(question);
        }

        // ADD QUESTION POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuestion(Question question)
        {
            // Server-side validation
            if (question.SubjectId == 0 || string.IsNullOrWhiteSpace(question.QuestionText))
            {
                ModelState.AddModelError("", "Subject and Question Text are required.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Subjects = new SelectList(await _db.Subjects.ToListAsync(), "SubjectId", "SubjectName", question.SubjectId);
                ViewBag.Subject = await _db.Subjects.FindAsync(question.SubjectId);
                return View(question);
            }

            try
            {
                question.CreatedAt = DateTime.UtcNow;
                _db.Questions.Add(question);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Question added successfully!";
                return RedirectToAction("ViewQuestions", new { subjectId = question.SubjectId });
            }
            catch (Exception ex)
            {
                // Log the error here if you have logging
                ModelState.AddModelError("", $"Error saving question: {ex.Message}");
                ViewBag.Subjects = new SelectList(await _db.Subjects.ToListAsync(), "SubjectId", "SubjectName", question.SubjectId);
                ViewBag.Subject = await _db.Subjects.FindAsync(question.SubjectId);
                return View(question);
            }
        }

        // EDIT QUESTION PAGE
        public async Task<IActionResult> EditQuestion(int id)
        {
            var q = await _db.Questions.FindAsync(id);
            if (q == null) return NotFound();

            ViewBag.Subjects = new SelectList(await _db.Subjects.ToListAsync(), "SubjectId", "SubjectName", q.SubjectId);
            return View(q);
        }

        // EDIT QUESTION POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuestion(Question question)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Subjects = new SelectList(await _db.Subjects.ToListAsync(), "SubjectId", "SubjectName", question.SubjectId);
                return View(question);
            }

            try
            {
                _db.Questions.Update(question);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Question updated successfully!";
                return RedirectToAction("ViewQuestions", new { subjectId = question.SubjectId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating question: {ex.Message}");
                ViewBag.Subjects = new SelectList(await _db.Subjects.ToListAsync(), "SubjectId", "SubjectName", question.SubjectId);
                return View(question);
            }
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
                TempData["Success"] = "Question deleted successfully!";
                return RedirectToAction("ViewQuestions", new { subjectId });
            }

            TempData["Error"] = "Question not found.";
            return RedirectToAction("ManageQuestions");
        }

        // DELETE SUBJECT AND ITS QUESTIONS
        public IActionResult DeleteSubject(int id)
        {
            var subject = _db.Subjects.Find(id);
            if (subject != null)
            {
                var questions = _db.Questions.Where(q => q.SubjectId == id).ToList();
                _db.Questions.RemoveRange(questions);

                _db.Subjects.Remove(subject);
                _db.SaveChanges();
                TempData["Success"] = "Subject and its questions deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Subject not found.";
            }

            return RedirectToAction("ManageQuestions");
        }
    }
}