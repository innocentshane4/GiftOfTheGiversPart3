using GiftOfTheGivers.Models;

namespace GiftOfTheGivers.ViewModels
{
    public class DonorDashboardViewModel
    {
        public string DonorName { get; set; } = string.Empty;
        public decimal TotalDonations { get; set; }
        public Donation? MostRecentDonation { get; set; }
        public int TaxCertificatesCount { get; set; }
        public List<Donation> RecentDonations { get; set; } = new();
    }

    public class EmployeeDashboardViewModel
    {
        public string EmployeeName { get; set; } = string.Empty;
        public int ActiveProjectsCount { get; set; }
        public int RegisteredVolunteersCount { get; set; }
        public decimal TotalDonationValue { get; set; }
        public List<ProjectUpdate> RecentUpdates { get; set; } = new();
    }

    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public decimal TotalDonationValue { get; set; }
        public int TotalProjects { get; set; }
        public int ActiveVolunteers { get; set; }
        public List<AuditLog> RecentActivity { get; set; } = new();
    }

    public class ProjectFormViewModel
    {
        public int? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public ProjectCategory Category { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime ExpectedCompletionDate { get; set; } = DateTime.Today.AddMonths(3);
        public decimal Budget { get; set; }
        public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
        public int ProgressPercentage { get; set; }
    }

    public class ProjectUpdateFormViewModel
    {
        public int ReliefProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<ReliefProject> AvailableProjects { get; set; } = new();
    }
}
