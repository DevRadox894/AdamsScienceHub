using System.Linq;
using System.Threading.Tasks;
using AdamsScienceHub.Data;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;

namespace AdamsScienceHub.Controllers
{
    public class SubjectsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public SubjectsController(ApplicationDbContext db) { _db = db; }

        // GET: /Subjects/Explore/{subjectId}
        public async Task<IActionResult> Explore(int id)
        {
            var subject = await _db.Subjects.FindAsync(id);
            if (subject == null) return NotFound();

            // topics = materials for this subject
            var topics = await _db.Materials
             .Where(m => m.SubjectId == id)
              .OrderBy(m => m.UploadedAt) // oldest first
             .ToListAsync();


            ViewBag.Subject = subject;
            return View(topics);
        }

        // GET: /Subjects/Read/{materialId}
        public async Task<IActionResult> Read(int id)
        {
            var material = await _db.Materials
                .Include(m => m.Subject)
                .FirstOrDefaultAsync(m => m.MaterialId == id);

            if (material == null) return NotFound();

            // get all materials for subject to enable prev/next
            var siblings = await _db.Materials
                .Where(m => m.SubjectId == material.SubjectId)
                .OrderBy(m => m.MaterialId)
                .ToListAsync();

            var index = siblings.FindIndex(x => x.MaterialId == material.MaterialId);
            if (index == -1) return NotFound();

            ViewBag.Siblings = siblings;
            ViewBag.Index = index;
            return View(material);
        }

        // GET: /Subjects/Pdf/{materialId}
        public async Task<IActionResult> Pdf(int id)
        {
            var material = await _db.Materials.FindAsync(id);
            if (material == null || string.IsNullOrEmpty(material.FilePath))
                return NotFound();

            using var http = new HttpClient();
            var pdfBytes = await http.GetByteArrayAsync(material.FilePath);

            return File(pdfBytes, "application/pdf");
        }


        // POST: /Subjects/RecordTime  (AJAX)
        [HttpPost]
        public async Task<IActionResult> RecordTime(int subjectId, int seconds)
        {
            // If you use int userId from session:
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null) return Unauthorized();

            var prog = await _db.UserSubjectProgress
                .FirstOrDefaultAsync(p => p.UserId == user.Id && p.SubjectId == subjectId);

            if (prog == null)
            {
                prog = new Models.UserSubjectProgress
                {
                    UserId = user.Id,
                    SubjectId = subjectId,
                    TotalSeconds = seconds,
                    LastUpdated = DateTime.UtcNow
                };
                _db.UserSubjectProgress.Add(prog);
            }
            else
            {
                prog.TotalSeconds += seconds;
                prog.LastUpdated = DateTime.UtcNow;
                _db.UserSubjectProgress.Update(prog);
            }

            await _db.SaveChangesAsync();
            return Ok(new { totalSeconds = prog.TotalSeconds });
        }
    }
}
