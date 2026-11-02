using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using GiftOfTheGivers.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// This application currently uses SQLite. A SQL Server-style value can be
// supplied by user secrets or an environment variable and would otherwise be
// passed to UseSqlite(), which rejects keywords such as "server".
var configuredConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var sqliteConnectionString = IsSqlServerConnectionString(configuredConnectionString)
    ? "Data Source=GiftOfTheGivers.db"
    : configuredConnectionString ?? "Data Source=GiftOfTheGivers.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(sqliteConnectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength        = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase      = true;
    options.Password.RequireLowercase      = true;
    options.Password.RequireDigit          = true;
    options.User.RequireUniqueEmail        = true;
    options.SignIn.RequireConfirmedAccount  = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath        = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan   = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DonorOnly",       policy => policy.RequireRole(UserRoles.Donor));
    options.AddPolicy("EmployeeOnly",    policy => policy.RequireRole(UserRoles.Employee));
    options.AddPolicy("AdminOnly",       policy => policy.RequireRole(UserRoles.Admin));
    options.AddPolicy("EmployeeOrAdmin", policy => policy.RequireRole(UserRoles.Employee, UserRoles.Admin));
});

builder.Services.AddScoped<IReferenceNumberService, ReferenceNumberService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await DbInitializer.SeedAsync(scope.ServiceProvider, app.Configuration);
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

app.MapControllerRoute(
    name: "adminLogin",
    pattern: "admin/login",
    defaults: new { controller = "AdminAccount", action = "Login" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static bool IsSqlServerConnectionString(string? connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
        return false;

    var sqlServerKeywords = new[]
    {
        "server=", "initial catalog=", "database=", "integrated security=",
        "trusted_connection=", "user id=", "uid=", "password=", "pwd=",
        "multipleactiveresultsets="
    };

    // Data Source= is valid for SQLite, so it is intentionally not listed.
    return sqlServerKeywords.Any(keyword =>
        connectionString.Contains(keyword, StringComparison.OrdinalIgnoreCase));
}
