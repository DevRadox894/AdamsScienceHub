using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AdamsScienceHub.Data;
using AdamsScienceHub.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace AdamsScienceHub.Controllers
{
    public class AdminMaterialsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly Cloudinary _cloudinary;

        public AdminMaterialsController(ApplicationDbContext db, Cloudinary cloudinary)
        {
            _db = db;
            _cloudinary = cloudinary;
        }

        // GET: /AdminMaterials/ManageMaterials
        public async Task<IActionResult> ManageMaterials()
        {
            var items = await _db.Materials
                .Include(m => m.Subject)
                .OrderBy(m => m.UploadedAt)
                .ToListAsync();
            return View(items);
        }

        // GET: /AdminMaterials/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Subjects = await _db.Subjects.ToListAsync();
            return View();
        }

        // POST: /AdminMaterials/Create
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

            var ext = Path.GetExtension(PdfFile.FileName).ToLowerInvariant();
            if (ext != ".pdf")
            {
                ModelState.AddModelError(nameof(PdfFile), "Only PDF files are allowed.");
                ViewBag.Subjects = await _db.Subjects.ToListAsync();
                return View();
            }

            // Upload PDF to Cloudinary
            var pdfUrl = await UploadPdfToCloudinary(PdfFile);
            if (string.IsNullOrEmpty(pdfUrl))
            {
                ModelState.AddModelError("", "PDF upload failed. Please try again.");
                ViewBag.Subjects = await _db.Subjects.ToListAsync();
                return View();
            }

            var material = new Material
            {
                SubjectId = SubjectId,
                TopicTitle = TopicTitle.Trim(),
                FilePath = pdfUrl,
                VideoUrl = string.IsNullOrWhiteSpace(VideoUrl) ? null : VideoUrl.Trim(),
                PageCount = 0,
                UploadedAt = DateTime.UtcNow
            };

            _db.Materials.Add(material);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(ManageMaterials));
        }

        // GET: /AdminMaterials/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var material = await _db.Materials.FindAsync(id);
            if (material == null) return NotFound();

            ViewBag.Subjects = await _db.Subjects.ToListAsync();
            return View(material);
        }

        // POST: /AdminMaterials/Edit/{id}
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
            material.UploadedAt = DateTime.UtcNow;

            if (PdfFile != null && PdfFile.Length > 0)
            {
                var ext = Path.GetExtension(PdfFile.FileName).ToLowerInvariant();
                if (ext != ".pdf")
                {
                    ModelState.AddModelError(nameof(PdfFile), "Only PDF files are allowed.");
                    ViewBag.Subjects = await _db.Subjects.ToListAsync();
                    return View(material);
                }

                // Delete old PDF from Cloudinary
                if (!string.IsNullOrEmpty(material.FilePath))
                {
                    await DeletePdfFromCloudinary(material.FilePath);
                }

                // Upload new PDF
                material.FilePath = await UploadPdfToCloudinary(PdfFile);
            }

            _db.Materials.Update(material);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(ManageMaterials));
        }

        // POST: /AdminMaterials/DeleteConfirmed/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var material = await _db.Materials.FindAsync(id);
            if (material != null)
            {
                // Delete PDF from Cloudinary
                if (!string.IsNullOrEmpty(material.FilePath))
                {
                    await DeletePdfFromCloudinary(material.FilePath);
                }

                _db.Materials.Remove(material);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ManageMaterials));
        }

        // Helper: Upload PDF to Cloudinary
        private async Task<string> UploadPdfToCloudinary(IFormFile file)
        {
            if (file == null || file.Length == 0) return string.Empty;

            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                Folder = "materials"
                // ResourceType is automatically set to Raw
            };

            var result = await _cloudinary.UploadAsync(uploadParams);
            return result.SecureUrl?.ToString() ?? string.Empty;
        }


        // Helper: Delete PDF from Cloudinary
        private async Task DeletePdfFromCloudinary(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl)) return;

            try
            {
                var uri = new Uri(fileUrl);
                var segments = uri.AbsolutePath.Split('/');
                // e.g., /v168943/materials/myfile -> skip /v168943/
                var publicId = string.Join("/", segments.Skip(2)).Replace(".pdf", "");
                await _cloudinary.DestroyAsync(new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Raw
                });
            }
            catch
            {
                // ignore errors if deletion fails
            }
        }
    }
}
