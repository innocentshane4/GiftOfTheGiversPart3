using GiftOfTheGivers.Models;

namespace GiftOfTheGivers.ViewModels
{
    public class ProjectListViewModel
    {
        public string? SearchTerm { get; set; }
        public ProjectStatus? StatusFilter { get; set; }
        public ProjectCategory? CategoryFilter { get; set; }
        public List<ReliefProject> Projects { get; set; } = new();
    }
}
