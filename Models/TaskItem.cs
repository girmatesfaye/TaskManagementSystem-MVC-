
using System.ComponentModel.DataAnnotations;

public class TaskItem
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(1000, MinimumLength = 5, ErrorMessage = "Description must be between 5 and 1000 characters.")]
    public string Description { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Due Date")]
    public DateTime DueDate { get; set; }

    [Required(ErrorMessage = "Priority is required.")]
    [RegularExpression("Low|Medium|High", ErrorMessage = "Priority must be Low, Medium, or High.")]
    public string Priority { get; set; } = string.Empty;

    [Required(ErrorMessage = "Status is required.")]
    [RegularExpression("Pending|Completed", ErrorMessage = "Status must be Pending or Completed.")]
    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
}