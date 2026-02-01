using AdamsScienceHub.Data;
using AdamsScienceHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System;
using System.IO;
using System.Threading.Tasks;

public class AdminSubjectController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly Cloudinary _cloudinary;


    public AdminSubjectController(ApplicationDbContext db, Cloudinary cloudinary)
    {
        _db = db;
        _cloudinary = cloudinary;
    }


    // GET: /AdminSubject/ManageSubjects
    public async Task<IActionResult> ManageSubjects()
    {
        var subjects = await _db.Subjects.ToListAsync();
        return View(subjects);
    }

    // GET: /AdminSubject/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /AdminSubject/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string SubjectName, IFormFile ImageFile)
    {
        if (string.IsNullOrWhiteSpace(SubjectName))
        {
            ModelState.AddModelError(nameof(SubjectName), "Subject name is required.");
            return View();
        }

        var subject = new Subject
        {
            SubjectName = SubjectName.Trim()
        };

        if (ImageFile != null && ImageFile.Length > 0)
        {
            var uploadResult = await UploadToCloudinary(ImageFile);
            if (uploadResult != null)
            {
                subject.ImagePath = uploadResult;
            }
        }

        _db.Subjects.Add(subject);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(ManageSubjects));
    }

    // GET: /AdminSubject/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var subject = await _db.Subjects.FindAsync(id);
        if (subject == null)
            return NotFound();

        return View(subject);
    }

    // POST: /AdminSubject/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string SubjectName, IFormFile ImageFile)
    {
        var subject = await _db.Subjects.FindAsync(id);
        if (subject == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(SubjectName))
        {
            ModelState.AddModelError(nameof(SubjectName), "Subject name is required.");
            return View(subject);
        }

        subject.SubjectName = SubjectName.Trim();

        if (ImageFile != null && ImageFile.Length > 0)
        {
            // Optional: delete old image from Cloudinary
            if (!string.IsNullOrEmpty(subject.ImagePath))
            {
                await DeleteFromCloudinary(subject.ImagePath);
            }

            var uploadResult = await UploadToCloudinary(ImageFile);
            if (uploadResult != null)
            {
                subject.ImagePath = uploadResult;
            }
        }

        _db.Subjects.Update(subject);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(ManageSubjects));
    }

    // POST: /AdminSubject/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var subject = await _db.Subjects.FindAsync(id);
        if (subject != null)
        {
            if (!string.IsNullOrEmpty(subject.ImagePath))
            {
                await DeleteFromCloudinary(subject.ImagePath);
            }

            _db.Subjects.Remove(subject);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(ManageSubjects));
    }

    // Helper: Upload image to Cloudinary
    private async Task<string?> UploadToCloudinary(IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "subjects"
            };
            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception(result.Error?.Message ?? "Cloudinary upload failed");
            }

            return result.SecureUrl.ToString();
        }
        catch (Exception ex)
        {
            // Log error to console or database
            Console.WriteLine("Cloudinary upload error: " + ex.Message);

            // Optionally show a friendly message to the user
            TempData["UploadError"] = "Failed to upload image. Please try again.";

            return null; // prevent crash
        }
    }


    // Helper: Delete image from Cloudinary
    private async Task DeleteFromCloudinary(string imageUrl)
    {
        try
        {
            var uri = new Uri(imageUrl);
            var publicId = Path.GetFileNameWithoutExtension(uri.AbsolutePath);
            await _cloudinary.DestroyAsync(new DeletionParams(publicId) { ResourceType = ResourceType.Image });
        }
        catch
        {
            // ignore errors if deletion fails
        }
    }
}
