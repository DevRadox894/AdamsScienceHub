using AdamsScienceHub.Data;
using AdamsScienceHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

public class AdminSubjectController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly string _subjectsFolder;

    public AdminSubjectController(ApplicationDbContext db)
    {
        _db = db;

        _subjectsFolder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "subjects"
        );

        if (!Directory.Exists(_subjectsFolder))
        {
            Directory.CreateDirectory(_subjectsFolder);
        }
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
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
            var savePath = Path.Combine(_subjectsFolder, fileName);

            using (var fs = new FileStream(savePath, FileMode.Create))
            {
                await ImageFile.CopyToAsync(fs);
            }

            subject.ImagePath = $"/subjects/{fileName}";
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
            if (!string.IsNullOrEmpty(subject.ImagePath))
            {
                var oldPath = subject.ImagePath.TrimStart('/');
                var fullOldPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    oldPath
                );

                if (System.IO.File.Exists(fullOldPath))
                {
                    System.IO.File.Delete(fullOldPath);
                }
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
            var savePath = Path.Combine(_subjectsFolder, fileName);

            using (var fs = new FileStream(savePath, FileMode.Create))
            {
                await ImageFile.CopyToAsync(fs);
            }

            subject.ImagePath = $"/subjects/{fileName}";
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
                var path = subject.ImagePath.TrimStart('/');
                var fullPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    path
                );

                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }

            _db.Subjects.Remove(subject);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(ManageSubjects));
    }
}
