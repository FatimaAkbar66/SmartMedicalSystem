using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SmartMedicalSystem.Data;
using SmartMedicalSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// ─── MVC ───────────────────────────────────────
builder.Services.AddControllersWithViews();

// ─── DATABASE ──────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ─── AUTHENTICATION ────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.Name = "SmartMedAuth";
    });

builder.Services.AddAuthorization();

// ─── SESSION ───────────────────────────────────
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ─── SERVICES ──────────────────────────────────
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<PrescriptionValidationService>();
builder.Services.AddScoped<PrescriptionPdfService>();
builder.Services.AddSingleton<RiskPredictionService>();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ─── MIDDLEWARE ─────────────────────────────────
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

// ─── AUTO CREATE DATABASE ───────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
                  .GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

// ─── ROUTES ─────────────────────────────────────
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();