using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class TasksController : Controller
{
	private readonly ApplicationDbContext _context;
	private readonly UserManager<IdentityUser> _userManager;

	public TasksController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
	{
		_context = context;
		_userManager = userManager;
	}

	// GET /tasks: loads only the signed-in user's tasks, applies ordering, and sends the list to the view.
	[HttpGet("/tasks")]
	public async Task<IActionResult> Index()
	{
		var userId = _userManager.GetUserId(User);
		if (string.IsNullOrEmpty(userId))
		{
			return Challenge();
		}

		var tasks = await _context.TaskItems
			.Where(t => t.UserId == userId)
			.OrderBy(t => t.DueDate)
			.ThenByDescending(t => t.CreatedAt)
			.ToListAsync();

		return View(tasks);
	}

	// GET /tasks/details/{id}: loads only the signed-in user's task by id and sends it to the details view.
	[HttpGet("/tasks/details/{id:int}")]
	public async Task<IActionResult> Details(int id)
	{
		var userId = _userManager.GetUserId(User);
		if (string.IsNullOrEmpty(userId))
		{
			return Challenge();
		}

		var taskItem = await _context.TaskItems
			.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

		if (taskItem == null)
		{
			return NotFound();
		}

		return View(taskItem);
	}

	// GET /tasks/create: returns the create form for the authenticated user.
	[HttpGet("/tasks/create")]
	public IActionResult Create()
	{
		return View();
	}

	// POST /tasks/create: validates input, sets current user ownership, saves, and redirects.
	[HttpPost("/tasks/create")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create([Bind("Title,Description,DueDate,Priority")] TaskItem taskItem)
	{
		var userId = _userManager.GetUserId(User);
		if (string.IsNullOrEmpty(userId))
		{
			return Challenge();
		}

		taskItem.Title = (taskItem.Title ?? string.Empty).Trim();
		taskItem.Description = (taskItem.Description ?? string.Empty).Trim();
		taskItem.Priority = string.IsNullOrWhiteSpace(taskItem.Priority)
			? "Medium"
			: taskItem.Priority.Trim();

		if (string.IsNullOrWhiteSpace(taskItem.Title))
		{
			ModelState.AddModelError(nameof(TaskItem.Title), "Title is required.");
		}

		if (!ModelState.IsValid)
		{
			return View(taskItem);
		}

		taskItem.Status = "Pending";
		taskItem.CreatedAt = DateTime.UtcNow;
		taskItem.UserId = userId;

		_context.Add(taskItem);
		await _context.SaveChangesAsync();

		return RedirectToAction(nameof(Index));
	}

	// GET /tasks/edit/{id}: loads only the signed-in user's task and sends it to the edit view.
	[HttpGet("/tasks/edit/{id:int}")]
	public async Task<IActionResult> Edit(int id)
	{
		var userId = _userManager.GetUserId(User);
		if (string.IsNullOrEmpty(userId))
		{
			return Challenge();
		}

		var taskItem = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
		if (taskItem == null)
		{
			return NotFound();
		}

		return View(taskItem);
	}

	// POST /tasks/edit/{id}: validates and updates only the signed-in user's task, then redirects.
	[HttpPost("/tasks/edit/{id:int}")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,DueDate,Priority")] TaskItem taskItem)
	{
		var userId = _userManager.GetUserId(User);
		if (string.IsNullOrEmpty(userId))
		{
			return Challenge();
		}

		if (id != taskItem.Id)
		{
			return NotFound();
		}

		var existingTask = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
		if (existingTask == null)
		{
			return NotFound();
		}

		existingTask.Title = (taskItem.Title ?? string.Empty).Trim();
		existingTask.Description = (taskItem.Description ?? string.Empty).Trim();
		existingTask.DueDate = taskItem.DueDate;
		existingTask.Priority = string.IsNullOrWhiteSpace(taskItem.Priority)
			? "Medium"
			: taskItem.Priority.Trim();

		if (string.IsNullOrWhiteSpace(existingTask.Title))
		{
			ModelState.AddModelError(nameof(TaskItem.Title), "Title is required.");
		}

		if (!ModelState.IsValid)
		{
			return View(existingTask);
		}

		try
		{
			_context.Update(existingTask);
			await _context.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			if (!TaskItemExists(taskItem.Id))
			{
				return NotFound();
			}

			throw;
		}

		return RedirectToAction(nameof(Index));
	}

	// GET /tasks/delete/{id}: loads only the signed-in user's task for delete confirmation.
	[HttpGet("/tasks/delete/{id:int}")]
	public async Task<IActionResult> Delete(int id)
	{
		var userId = _userManager.GetUserId(User);
		if (string.IsNullOrEmpty(userId))
		{
			return Challenge();
		}

		var taskItem = await _context.TaskItems
			.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

		if (taskItem == null)
		{
			return NotFound();
		}

		return View(taskItem);
	}

	// POST /tasks/delete/{id}: deletes only the signed-in user's task and redirects.
	[HttpPost("/tasks/delete/{id:int}"), ActionName("Delete")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> DeleteConfirmed(int id)
	{
		var userId = _userManager.GetUserId(User);
		if (string.IsNullOrEmpty(userId))
		{
			return Challenge();
		}

		var taskItem = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
		if (taskItem == null)
		{
			return NotFound();
		}

		_context.TaskItems.Remove(taskItem);
		await _context.SaveChangesAsync();

		return RedirectToAction(nameof(Index));
	}

	// POST /tasks/toggle-status/{id}: toggles status only for the signed-in user's task and redirects.
	[HttpPost("/tasks/toggle-status/{id:int}")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> ToggleStatus(int id)
	{
		var userId = _userManager.GetUserId(User);
		if (string.IsNullOrEmpty(userId))
		{
			return Challenge();
		}

		var taskItem = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
		if (taskItem == null)
		{
			return NotFound();
		}

		var currentStatus = (taskItem.Status ?? string.Empty).Trim();
		taskItem.Status = currentStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase)
			? "Pending"
			: "Completed";

		await _context.SaveChangesAsync();

		return RedirectToAction(nameof(Index));
	}

	private bool TaskItemExists(int id)
	{
		var userId = _userManager.GetUserId(User);
		if (string.IsNullOrEmpty(userId))
		{
			return false;
		}

		return _context.TaskItems.Any(e => e.Id == id && e.UserId == userId);
	}
}
