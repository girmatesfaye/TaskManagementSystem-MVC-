namespace TaskManagementSystem.Models;

public class DashboardViewModel
{
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
    public int OverdueTasks { get; set; }
    public List<global::TaskItem> RecentTasks { get; set; } = new();
}