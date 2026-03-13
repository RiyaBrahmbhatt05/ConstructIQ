using ConstructionSimulator.Data;
using ConstructionSimulator.Models;
using ConstructionSimulator.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Authentication Service
builder.Services.AddScoped<AuthenticationService>();

// Simulation Services
builder.Services.AddScoped<SimulationEngine>();
builder.Services.AddScoped<ConflictDetector>();
builder.Services.AddScoped<CostCalculator>();

// Session Configuration
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();

    if (!context.Crews.Any())
    {
        Console.WriteLine("Seeding default crews...");
        context.Crews.AddRange(
            new Crew { Name = "Electrical Crew", SkillType = "Electrical", TeamSize = 5, HourlyRate = 80, IsAvailable = true },
            new Crew { Name = "Plumbing Crew", SkillType = "Plumbing", TeamSize = 4, HourlyRate = 75, IsAvailable = true },
            new Crew { Name = "Concrete Crew", SkillType = "Concrete", TeamSize = 6, HourlyRate = 90, IsAvailable = true },
            new Crew { Name = "Carpentry Crew", SkillType = "Carpentry", TeamSize = 5, HourlyRate = 70, IsAvailable = true }
        );
    }

    if (!context.Permits.Any())
    {
        context.Permits.AddRange(
            new Permit { Type = "Electrical Permit", Status = "Pending", Fee = 300, IssuingAuthority = "City Office" },
            new Permit { Type = "Building Permit", Status = "Pending", Fee = 500, IssuingAuthority = "City Office" },
            new Permit { Type = "Environmental Permit", Status = "Pending", Fee = 400, IssuingAuthority = "Environmental Department" }
        );
    }

    if (!context.Materials.Any())
    {
        Console.WriteLine("Seeding default materials...");

        context.Materials.AddRange(
            new Material
            {
                Name = "Concrete",
                Description = "Standard structural concrete",
                Quantity = 100,
                UnitType = "Cubic Meter",
                CostPerUnit = 120,
                LeadTimeDays = 2,
                InStock = true
            },
            new Material
            {
                Name = "Steel Beams",
                Description = "Structural steel beams",
                Quantity = 50,
                UnitType = "Pieces",
                CostPerUnit = 300,
                LeadTimeDays = 5,
                InStock = true
            },
            new Material
            {
                Name = "Lumber",
                Description = "Construction grade lumber",
                Quantity = 200,
                UnitType = "Boards",
                CostPerUnit = 40,
                LeadTimeDays = 3,
                InStock = true
            }
        );
    }
    context.SaveChanges();
}

// Configure HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Landing}/{action=Index}/{id?}");

app.Run();