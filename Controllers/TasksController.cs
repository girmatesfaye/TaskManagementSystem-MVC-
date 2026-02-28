using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class TasksController : Controller
{
	private readonly ApplicationDbContext _context;

	public TasksController(ApplicationDbContext context)
	{
		_context = context;
	}

	// GET /tasks: loads task items, applies ordering logic, and sends the list to the view.
	[HttpGet("/tasks")]
	public async Task<IActionResult> Index()
	{
		var tasks = await _context.TaskItems
			.OrderBy(t => t.DueDate)
			.ThenByDescending(t => t.CreatedAt)
			.ToListAsync();

		return View(tasks);
	}

	// GET /tasks/details/{id}: loads one task by id and sends it to the details view.
	[HttpGet("/tasks/details/{id:int}")]
	public async Task<IActionResult> Details(int id)
	{
		var taskItem = await _context.TaskItems
			.FirstOrDefaultAsync(m => m.Id == id);

		if (taskItem == null)
		{
			return NotFound();
		}

		return View(taskItem);
	}

	// GET /tasks/create: returns the empty create form view.
	[HttpGet("/tasks/create")]
	public IActionResult Create()
	{
		return View();
	}

	// POST /tasks/create: validates input, applies defaults, saves the task, and redirects.
	[HttpPost("/tasks/create")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create([Bind("Title,Description,DueDate,Priority")] TaskItem taskItem)
	{
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
		taskItem.UserId = 0;

		_context.Add(taskItem);
		await _context.SaveChangesAsync();

		return RedirectToAction(nameof(Index));
	}

	// GET /tasks/edit/{id}: loads an existing task and sends it to the edit view.
	[HttpGet("/tasks/edit/{id:int}")]
	public async Task<IActionResult> Edit(int id)
	{
		var taskItem = await _context.TaskItems.FindAsync(id);
		if (taskItem == null)
		{
			return NotFound();
		}

		return View(taskItem);
	}

	// POST /tasks/edit/{id}: validates update input, saves changes, and redirects.
	[HttpPost("/tasks/edit/{id:int}")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,DueDate,Priority")] TaskItem taskItem)
	{
		if (id != taskItem.Id)
		{
			return NotFound();
		}

		var existingTask = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == id);
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

	// GET /tasks/delete/{id}: loads a task for delete confirmation view.
	[HttpGet("/tasks/delete/{id:int}")]
	public async Task<IActionResult> Delete(int id)
	{
		var taskItem = await _context.TaskItems
			.FirstOrDefaultAsync(m => m.Id == id);

		if (taskItem == null)
		{
			return NotFound();
		}

		return View(taskItem);
	}

	// POST /tasks/delete/{id}: removes the task from DbContext and redirects.
	[HttpPost("/tasks/delete/{id:int}"), ActionName("Delete")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> DeleteConfirmed(int id)
	{
		var taskItem = await _context.TaskItems.FindAsync(id);
		if (taskItem == null)
		{
			return NotFound();
		}

		_context.TaskItems.Remove(taskItem);
		await _context.SaveChangesAsync();

		return RedirectToAction(nameof(Index));
	}

	// POST /tasks/toggle-status/{id}: toggles task status and redirects to the list.
	[HttpPost("/tasks/toggle-status/{id:int}")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> ToggleStatus(int id)
	{
		var taskItem = await _context.TaskItems.FindAsync(id);
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
		return _context.TaskItems.Any(e => e.Id == id);
	}
}
