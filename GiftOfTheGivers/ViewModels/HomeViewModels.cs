namespace GiftOfTheGivers.ViewModels
{
    public class HomeIndexViewModel
    {
        public int ReliefProjectsCount { get; set; }
        public int PeopleAssistedCount { get; set; }
        public int VolunteersCount { get; set; }
        public int CountriesSupportedCount { get; set; }
        public List<GiftOfTheGivers.Models.ReliefProject> FeaturedProjects { get; set; } = new();
    }
}
