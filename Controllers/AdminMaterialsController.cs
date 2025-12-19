using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AdamsScienceHub.Data;
using AdamsScienceHub.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdamsScienceHub.Controllers
{
    public class AdminMaterialsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly string _materialsFolder = Path.Combine("wwwroot", "materials");

        public AdminMaterialsController(ApplicationDbContext db)
        {
            _db = db;
            if (!Directory.Exists(_materialsFolder))
                Directory.CreateDirectory(_materialsFolder);
        }

        // GET: /AdminMaterials/ManageMaterials
        public async Task<IActionResult> ManageMaterials()
        {
            var items = await _db.Materials
                .Include(m => m.Subject)
                .OrderBy(m => m.UploadedAt) // oldest first
                .ToListAsync();
            return View(items);
        }

        // GET: Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Subjects = await _db.Subjects.ToListAsync();
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int SubjectId, string TopicTitle, IFormFile? PdfFile, string? VideoUrl)
        {
            if (string.IsNullOrWhiteSpace(TopicTitle) || SubjectId == 0)
            {
                ModelState.AddModelError(string.Empty, "Subject and Topic Title are required.");
                ViewBag.Subjects = await _db.Subjects.ToListAsync();
                return View();
            }

            if (PdfFile == null || PdfFile.Length == 0)
            {
                ModelState.AddModelError(nameof(PdfFile), "PDF file is required.");
                ViewBag.Subjects = await _db.Subjects.ToListAsync();
                return View();
            }

            // Validate PDF only
            var ext = Path.GetExtension(PdfFile.FileName).ToLowerInvariant();
            if (ext != ".pdf")
            {
                ModelState.AddModelError(nameof(PdfFile), "Only PDF files are allowed.");
                ViewBag.Subjects = await _db.Subjects.ToListAsync();
                return View();
            }

            var fileName = $"{Guid.NewGuid()}{ext}";
            var savePath = Path.Combine(_materialsFolder, fileName);
            using (var fs = new FileStream(savePath, FileMode.Create))
            {
                await PdfFile.CopyToAsync(fs);
            }

            var material = new Material
            {
                SubjectId = SubjectId,
                TopicTitle = TopicTitle.Trim(),
                FilePath = $"/materials/{fileName}",
                VideoUrl = string.IsNullOrWhiteSpace(VideoUrl) ? null : VideoUrl.Trim(),
                PageCount = 0, // ← ADDED: Default page count
                UploadedAt = DateTime.UtcNow // ← ADDED: Set upload timestamp
            };

            _db.Materials.Add(material);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(ManageMaterials));
        }

        // GET: Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var material = await _db.Materials.FindAsync(id);
            if (material == null) return NotFound();
            ViewBag.Subjects = await _db.Subjects.ToListAsync();
            return View(material);
        }

        // POST: Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int SubjectId, string TopicTitle, IFormFile? PdfFile, string? VideoUrl)
        {
            var material = await _db.Materials.FindAsync(id);
            if (material == null) return NotFound();

            if (string.IsNullOrWhiteSpace(TopicTitle) || SubjectId == 0)
            {
                ModelState.AddModelError(string.Empty, "Subject and Topic Title are required.");
                ViewBag.Subjects = await _db.Subjects.ToListAsync();
                return View(material);
            }

            material.SubjectId = SubjectId;
            material.TopicTitle = TopicTitle.Trim();
            material.VideoUrl = string.IsNullOrWhiteSpace(VideoUrl) ? null : VideoUrl.Trim();
            material.UploadedAt = DateTime.UtcNow; // ← ADDED: Update timestamp on edit

            if (PdfFile != null && PdfFile.Length > 0)
            {
                var ext = Path.GetExtension(PdfFile.FileName).ToLowerInvariant();
                if (ext != ".pdf")
                {
                    ModelState.AddModelError(nameof(PdfFile), "Only PDF files are allowed.");
                    ViewBag.Subjects = await _db.Subjects.ToListAsync();
                    return View(material);
                }

                // Delete old file
                if (!string.IsNullOrEmpty(material.FilePath))
                {
                    var old = material.FilePath.StartsWith("/") ? material.FilePath.TrimStart('/') : material.FilePath;
                    var fullOld = Path.Combine(Directory.GetCurrentDirectory(), old);
                    if (System.IO.File.Exists(fullOld)) System.IO.File.Delete(fullOld);
                }

                var fileName = $"{Guid.NewGuid()}{ext}";
                var savePath = Path.Combine(_materialsFolder, fileName);
                using (var fs = new FileStream(savePath, FileMode.Create))
                {
                    await PdfFile.CopyToAsync(fs);
                }
                material.FilePath = $"/materials/{fileName}";
            }

            _db.Materials.Update(material);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(ManageMaterials));
        }

        // POST: DeleteConfirmed/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var material = await _db.Materials.FindAsync(id);
            if (material != null)
            {
                // delete file
                if (!string.IsNullOrEmpty(material.FilePath))
                {
                    var path = material.FilePath.StartsWith("/") ? material.FilePath.TrimStart('/') : material.FilePath;
                    var full = Path.Combine(Directory.GetCurrentDirectory(), path);
                    if (System.IO.File.Exists(full)) System.IO.File.Delete(full);
                }

                _db.Materials.Remove(material);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ManageMaterials));
        }

    }
}