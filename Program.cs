using AdamsScienceHub.Data;
using AdamsScienceHub.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Database (SQLite for Development, PostgreSQL for Production)
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddDbContext<DataProtectionKeyContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DataProtectionKeyContext")));

}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddDbContext<DataProtectionKeyContext>(options =>
     options.UseNpgsql(builder.Configuration.GetConnectionString("DataProtectionKeyContext"))
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
 );
}
    // Persist Data Protection keys in PostgreSQL
    builder.Services.AddDataProtection()
    .SetApplicationName("AdamsScienceHub")
    .PersistKeysToDbContext<DataProtectionKeyContext>();

// Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });

// Cloudinary
var cloudinarySection = builder.Configuration.GetSection("Cloudinary");
var account = new Account(
    cloudinarySection["CloudName"],
    cloudinarySection["ApiKey"],
    cloudinarySection["ApiSecret"]
);
var cloudinary = new Cloudinary(account) { Api = { Secure = true } };
builder.Services.AddSingleton(cloudinary);

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Bind Render PORT safely
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Clear();
app.Urls.Add($"http://0.0.0.0:{port}");

// Run migrations only in Development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var keyDb = scope.ServiceProvider.GetRequiredService<DataProtectionKeyContext>();

    db.Database.Migrate();
    keyDb.Database.Migrate();

    DbSeeder.SeedAdmin(db);
}
else
{
    // ✅ In Production, do NOT auto-run migrations to avoid file watching / descriptor limits
    // You can apply migrations manually or via CI/CD pipeline
    Console.WriteLine("Running in Production - automatic migrations disabled.");
}

// Disable inotify watchers in production

app.Run();