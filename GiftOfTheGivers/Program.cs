
using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using GiftOfTheGivers.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Azure SQL Database connection
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "The DefaultConnection connection string is missing. " +
        "Please configure it in appsettings.json or environment variables."
    );
}

if (connectionString.Contains("{your_password}", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        "The Azure SQL password is not configured. Set the complete DefaultConnection " +
        "with .NET user-secrets before starting the application.");
}

// Fail early with a useful configuration error rather than allowing a
// malformed SQL Server connection string to fail during database seeding.
try
{
    _ = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
}
catch (ArgumentException ex)
{
    throw new InvalidOperationException(
        "The DefaultConnection connection string is invalid. " +
        "Check the SQL Server host, database, user, and password.", ex);
}

// Configure Entity Framework Core to use the SQL Server connection string
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)));

// ASP.NET Core Identity configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireDigit = true;

    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Cookie authentication configuration
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "DonorOnly",
        policy => policy.RequireRole(UserRoles.Donor));

    options.AddPolicy(
        "EmployeeOnly",
        policy => policy.RequireRole(UserRoles.Employee));

    options.AddPolicy(
        "AdminOnly",
        policy => policy.RequireRole(UserRoles.Admin));

    options.AddPolicy(
        "EmployeeOrAdmin",
        policy => policy.RequireRole(
            UserRoles.Employee,
            UserRoles.Admin));
});

// Application services
builder.Services.AddScoped<IReferenceNumberService, ReferenceNumberService>();

var app = builder.Build();

// Initialize and seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        await DbInitializer.SeedAsync(services, app.Configuration);
    }
    catch (Microsoft.EntityFrameworkCore.Storage.RetryLimitExceededException ex)
    {
        throw new InvalidOperationException(
            "The application could not connect to Azure SQL Server. " +
            "Verify the Azure SQL server is running, its firewall allows this client IP, " +
            "DNS/network access is available, and the user-secrets connection string is correct.",
            ex);
    }
}

// HTTP request pipeline
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

// Admin login route
app.MapControllerRoute(
    name: "adminLogin",
    pattern: "admin/login",
    defaults: new
    {
        controller = "AdminAccount",
        action = "Login"
    });

// Default MVC route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
