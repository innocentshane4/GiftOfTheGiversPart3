using GiftOfTheGivers.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
        {
            var context     = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            // Create the SQL Server schema when it does not exist yet.
            // This project currently has no EF Core migrations.
            await context.Database.EnsureCreatedAsync();

            // 1. Roles
            foreach (var role in new[] { UserRoles.Donor, UserRoles.Employee, UserRoles.Admin })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            var s                = config.GetSection("SeedSettings");
            var adminEmail       = s["AdminEmail"]       ?? "admin@giftofthegivers.org";
            var adminPassword    = s["AdminPassword"]    ?? "Admin@12345";
            var employeeEmail    = s["EmployeeEmail"]    ?? "employee@giftofthegivers.org";
            var employeePassword = s["EmployeePassword"] ?? "Employee@12345";
            var donorEmail       = s["DonorEmail"]       ?? "donor@example.com";
            var donorPassword    = s["DonorPassword"]    ?? "Donor@12345";

            // 2. Admin
            var adminUser = await GetOrCreateUserAsync(userManager, adminEmail, adminPassword, "System Administrator", UserRoles.Admin);
            if (!context.Admins.Any(a => a.UserId == adminUser.Id))
            {
                context.Admins.Add(new Admin { UserId = adminUser.Id, ProvisionedDate = DateTime.UtcNow.AddMonths(-8) });
                await context.SaveChangesAsync();
            }

            // 3. Employees
            var emp1User = await GetOrCreateUserAsync(userManager, employeeEmail, employeePassword, "Naledi Khumalo", UserRoles.Employee);
            var emp2User = await GetOrCreateUserAsync(userManager, "employee2@giftofthegivers.org", employeePassword, "Sipho Dlamini", UserRoles.Employee);

            Employee emp1;
            if (!context.Employees.Any(e => e.UserId == emp1User.Id))
            {
                emp1 = new Employee { UserId = emp1User.Id, Position = "Relief Project Coordinator", HireDate = DateTime.UtcNow.AddYears(-2) };
                context.Employees.Add(emp1);
            }
            else { emp1 = context.Employees.First(e => e.UserId == emp1User.Id); }

            Employee emp2;
            if (!context.Employees.Any(e => e.UserId == emp2User.Id))
            {
                emp2 = new Employee { UserId = emp2User.Id, Position = "Field Operations Officer", HireDate = DateTime.UtcNow.AddYears(-1) };
                context.Employees.Add(emp2);
            }
            else { emp2 = context.Employees.First(e => e.UserId == emp2User.Id); }

            await context.SaveChangesAsync();

            // 4. Donors
            var donorUser = await GetOrCreateUserAsync(userManager, donorEmail, donorPassword, "Thandiwe Mokoena", UserRoles.Donor);
            Donor donor1;
            if (!context.Donors.Any(d => d.UserId == donorUser.Id))
            {
                donor1 = new Donor { UserId = donorUser.Id };
                context.Donors.Add(donor1);
                await context.SaveChangesAsync();
            }
            else { donor1 = context.Donors.First(d => d.UserId == donorUser.Id); }

            var donor2User = await GetOrCreateUserAsync(userManager, "donor2@example.com", donorPassword, "James van der Merwe", UserRoles.Donor);
            Donor donor2;
            if (!context.Donors.Any(d => d.UserId == donor2User.Id))
            {
                donor2 = new Donor { UserId = donor2User.Id };
                context.Donors.Add(donor2);
                await context.SaveChangesAsync();
            }
            else { donor2 = context.Donors.First(d => d.UserId == donor2User.Id); }

            // 5. Relief projects
            if (!context.ReliefProjects.Any())
            {
                var projects = new List<ReliefProject>
                {
                    new() { Name = "Flood Relief — KwaZulu-Natal",      Description = "Emergency shelter, clean water and food parcels for displaced families.",     Location = "Durban, KwaZulu-Natal",    Category = ProjectCategory.EmergencyRelief,   StartDate = DateTime.UtcNow.AddMonths(-2), ExpectedCompletionDate = DateTime.UtcNow.AddMonths(1),  Budget = 1_250_000m, Status = ProjectStatus.InProgress,  ProgressPercentage = 65,  CreatedByEmployeeId = emp1.Id },
                    new() { Name = "Community Food Security Programme",  Description = "Weekly food parcel distribution supporting vulnerable households.",           Location = "Gqeberha, Eastern Cape",   Category = ProjectCategory.FoodSecurity,      StartDate = DateTime.UtcNow.AddMonths(-6), ExpectedCompletionDate = DateTime.UtcNow.AddMonths(6),  Budget = 850_000m,   Status = ProjectStatus.InProgress,  ProgressPercentage = 40,  CreatedByEmployeeId = emp1.Id },
                    new() { Name = "Mobile Medical Clinic Rollout",      Description = "Deploying mobile clinics to underserved rural communities.",                  Location = "Limpopo Province",         Category = ProjectCategory.MedicalSupport,    StartDate = DateTime.UtcNow.AddMonths(1),  ExpectedCompletionDate = DateTime.UtcNow.AddMonths(9),  Budget = 2_100_000m, Status = ProjectStatus.Upcoming,    ProgressPercentage = 0,   CreatedByEmployeeId = emp2.Id },
                    new() { Name = "Drought Recovery — Northern Cape",   Description = "Borehole rehabilitation and livestock feed support for farming communities.", Location = "Kimberley, Northern Cape", Category = ProjectCategory.CommunityRecovery, StartDate = DateTime.UtcNow.AddMonths(-9), ExpectedCompletionDate = DateTime.UtcNow.AddMonths(-1), Budget = 640_000m,   Status = ProjectStatus.Completed,   ProgressPercentage = 100, CreatedByEmployeeId = emp2.Id },
                    new() { Name = "Winter Shelter Kits — Free State",   Description = "Blankets, warm clothing and shelter reinforcement kits for winter.",          Location = "Bloemfontein, Free State", Category = ProjectCategory.EmergencyRelief,   StartDate = DateTime.UtcNow.AddMonths(2),  ExpectedCompletionDate = DateTime.UtcNow.AddMonths(4),  Budget = 430_000m,   Status = ProjectStatus.Planning,    ProgressPercentage = 0,   CreatedByEmployeeId = emp1.Id },
                    new() { Name = "School Feeding Scheme Expansion",    Description = "Nutritious daily meals expanded to 12 additional primary schools.",           Location = "Mpumalanga Province",      Category = ProjectCategory.FoodSecurity,      StartDate = DateTime.UtcNow.AddMonths(-1), ExpectedCompletionDate = DateTime.UtcNow.AddMonths(5),  Budget = 980_000m,   Status = ProjectStatus.InProgress,  ProgressPercentage = 25,  CreatedByEmployeeId = emp2.Id },
                };
                context.ReliefProjects.AddRange(projects);
                await context.SaveChangesAsync();

                context.ProjectUpdates.AddRange(
                    new ProjectUpdate { Title = "1,200 food parcels delivered", Description = "Field team completed distribution across four affected wards.", PostedDate = DateTime.UtcNow.AddDays(-3),  ReliefProjectId = projects[0].Id, EmployeeId = emp1.Id },
                    new ProjectUpdate { Title = "Temporary shelters erected",   Description = "20 temporary shelters erected at the Umlazi community hall.",  PostedDate = DateTime.UtcNow.AddDays(-10), ReliefProjectId = projects[0].Id, EmployeeId = emp1.Id },
                    new ProjectUpdate { Title = "New community garden planted", Description = "Third community vegetable garden planted with local volunteers.",PostedDate = DateTime.UtcNow.AddDays(-5),  ReliefProjectId = projects[1].Id, EmployeeId = emp1.Id }
                );
                await context.SaveChangesAsync();
            }

            // 6. Donations
            if (!context.Donations.Any())
            {
                var flood = context.ReliefProjects.First(p => p.Name.Contains("Flood"));
                var food  = context.ReliefProjects.First(p => p.Name.Contains("Food Security"));

                var donations = new List<Donation>
                {
                    new() { Amount = 1000m, Currency = CurrencyCode.ZAR, DonationType = DonationType.OneTime,  IsAnonymous = false, DonorId = donor1.Id, ReliefProjectId = flood.Id, DonationDate = DateTime.UtcNow.AddDays(-20), ReferenceNumber = "GG-20261001-100234", Status = DonationStatus.Completed },
                    new() { Amount = 500m,  Currency = CurrencyCode.ZAR, DonationType = DonationType.Recurring, IsAnonymous = false, DonorId = donor1.Id, ReliefProjectId = food.Id,  DonationDate = DateTime.UtcNow.AddDays(-8),  ReferenceNumber = "GG-20261009-100317", Status = DonationStatus.Completed },
                    new() { Amount = 2500m, Currency = CurrencyCode.USD, DonationType = DonationType.OneTime,  IsAnonymous = false, DonorId = donor2.Id, ReliefProjectId = flood.Id, DonationDate = DateTime.UtcNow.AddDays(-2),  ReferenceNumber = "GG-20261014-100412", Status = DonationStatus.Completed },
                    new() { Amount = 250m,  Currency = CurrencyCode.ZAR, DonationType = DonationType.OneTime,  IsAnonymous = true,  DonorId = null,      ReliefProjectId = food.Id,  DonationDate = DateTime.UtcNow.AddDays(-1),  ReferenceNumber = "GG-20261015-100455", Status = DonationStatus.Completed },
                };
                context.Donations.AddRange(donations);
                await context.SaveChangesAsync();

                context.TaxCertificates.Add(new TaxCertificate
                {
                    CertificateNumber = "GG-TC-2026-0001",
                    IssuedDate        = donations[0].DonationDate.AddDays(1),
                    Amount            = donations[0].Amount,
                    Currency          = donations[0].Currency,
                    DonationId        = donations[0].Id
                });
                await context.SaveChangesAsync();
            }

            // 7. Volunteers
            if (!context.Volunteers.Any())
            {
                context.Volunteers.AddRange(
                    new Volunteer { FullName = "Lindiwe Ndlovu", Email = "lindiwe@example.com", Phone = "0712345678", Skills = "First Aid, Logistics",          Availability = "Weekends", PreferredArea = "Emergency Relief", RegistrationDate = DateTime.UtcNow.AddDays(-15), Status = VolunteerStatus.Approved },
                    new Volunteer { FullName = "Pieter Botha",   Email = "pieter@example.com",  Phone = "0823456789", Skills = "Driving, Warehouse Management", Availability = "Flexible", PreferredArea = "Food Security",    RegistrationDate = DateTime.UtcNow.AddDays(-4),  Status = VolunteerStatus.Pending  },
                    new Volunteer { FullName = "Amahle Zulu",    Email = "amahle@example.com",  Phone = "0734567890", Skills = "Nursing, Community Outreach",   Availability = "Weekdays", PreferredArea = "Medical Support",  RegistrationDate = DateTime.UtcNow.AddDays(-1),  Status = VolunteerStatus.Pending  }
                );
                await context.SaveChangesAsync();
            }

            // 8. Audit log
            if (!context.AuditLogs.Any())
            {
                context.AuditLogs.AddRange(
                    new AuditLog { Action = "New donor registration",          EntityName = "Donor",         EntityId = donor2.Id.ToString(), UserId = donor2User.Id, UserDisplayName = donor2User.FullName, Timestamp = DateTime.UtcNow.AddDays(-2),   Status = "Success" },
                    new AuditLog { Action = "Employee account provisioned",    EntityName = "Employee",      EntityId = emp2.Id.ToString(),   UserId = adminUser.Id,  UserDisplayName = adminUser.FullName,   Timestamp = DateTime.UtcNow.AddDays(-30),  Status = "Success" },
                    new AuditLog { Action = "Project created",                 EntityName = "ReliefProject",                                  UserId = emp1User.Id,   UserDisplayName = emp1User.FullName,    Timestamp = DateTime.UtcNow.AddMonths(-2), Status = "Success" },
                    new AuditLog { Action = "Donation recorded",               EntityName = "Donation",                                       UserId = donor2User.Id, UserDisplayName = donor2User.FullName,  Timestamp = DateTime.UtcNow.AddDays(-2),   Status = "Success" },
                    new AuditLog { Action = "Volunteer registration submitted", EntityName = "Volunteer",                                      UserDisplayName = "Amahle Zulu",                                Timestamp = DateTime.UtcNow.AddDays(-1),   Status = "Success" }
                );
                await context.SaveChangesAsync();
            }
        }

        private static async Task<ApplicationUser> GetOrCreateUserAsync(
            UserManager<ApplicationUser> userManager,
            string email, string password, string fullName, string role)
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null) return existing;

            var user = new ApplicationUser
            {
                UserName = email, Email = email, EmailConfirmed = true,
                FullName = fullName, Role = role, CreatedDate = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Failed to seed {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            await userManager.AddToRoleAsync(user, role);
            return user;
        }
    }
}
