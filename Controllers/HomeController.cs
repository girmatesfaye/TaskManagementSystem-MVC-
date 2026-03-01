using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public HomeController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel();
        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(userId))
        {
            return View(model);
        }

        var userTasks = _context.TaskItems.Where(t => t.UserId == userId);
        var today = DateTime.UtcNow.Date;

        model.TotalTasks = await userTasks.CountAsync();
        model.CompletedTasks = await userTasks.CountAsync(t => t.Status == "Completed");
        model.PendingTasks = await userTasks.CountAsync(t => t.Status != "Completed");
        model.OverdueTasks = await userTasks.CountAsync(t => t.DueDate.Date < today && t.Status != "Completed");
        model.RecentTasks = await userTasks
            .OrderByDescending(t => t.CreatedAt)
            .Take(5)
            .ToListAsync();

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
