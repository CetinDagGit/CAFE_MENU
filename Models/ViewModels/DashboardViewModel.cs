namespace CAFE_MENU.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Copilot: Create a dictionary to hold Category name and the number of items in that category
        public Dictionary<string, int> CategoryCount { get; set; }

        // Copilot: Create a property to hold Daily Total Revenue
        public decimal DailyTotalRevenue { get; set; }
    }
}
