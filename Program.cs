using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MuseumSystem.Models;
using MuseumSystem;

var builder = WebApplication.CreateBuilder(args);

// --- DATABASE CONNECTION ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// --- IDENTITY & ROLES ---
builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    // so password admin123 will be accepted
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 3;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

// --- SESSION DATA ---
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// --- THE TRIGGER: THIS RUNS THE SEEDER ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try 
    {
        await DbSeeder.SeedDefaultData(services);
    }
    catch (Exception ex)
    {
        Console.WriteLine("An error occurred seeding the DB: " + ex.Message);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); 

app.Run();