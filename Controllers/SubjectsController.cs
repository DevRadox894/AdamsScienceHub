using System.Linq;
using System.Threading.Tasks;
using AdamsScienceHub.Data;
using Microsoft.AspNetCore.Mvc;
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

            var topics = await _db.Materials
                .Where(m => m.SubjectId == id)
                .OrderBy(m => m.UploadedAt)
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

        // ✅ Removed Pdf action — not needed for public Cloudinary PDFs

        // POST: /Subjects/RecordTime
        [HttpPost]
        public async Task<IActionResult> RecordTime(int subjectId, int seconds)
        {
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