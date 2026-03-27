using System;
using System.ComponentModel.DataAnnotations;

namespace ConstructionSimulator.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(256, MinimumLength = 6,
            ErrorMessage = "Password must be between 6 and 256 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(256, MinimumLength = 2,
            ErrorMessage = "Name must be between 2 and 256 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(256, MinimumLength = 6,
            ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;
    }

    public class ProfileViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(256, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 256 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [StringLength(256, MinimumLength = 6, ErrorMessage = "New Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "New passwords do not match")]
        public string? ConfirmNewPassword { get; set; }
    }

    public class DeleteProfileViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Range(typeof(bool), "true", "true", ErrorMessage = "Please confirm account deletion")]
        [Display(Name = "I understand this action is permanent")]
        public bool ConfirmDeletion { get; set; }
    }

    public class DashboardViewModel
    {
        public int TotalProjects { get; set; }
        public int ActiveProjects { get; set; }
        public int CompletedProjects { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal TotalActualCost { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }

        public List<Models.Project> RecentProjects { get; set; } = new List<Models.Project>();
        public List<Models.ProjectTask> UpcomingTasks { get; set; } = new List<Models.ProjectTask>();
        public List<ConflictAlert> Conflicts { get; set; } = new List<ConflictAlert>();
    }

    public class ProjectDetailsViewModel
    {
        public Models.Project? Project { get; set; }
        public List<Models.ProjectTask> Tasks { get; set; } = new List<Models.ProjectTask>();
        public List<Models.Crew> AvailableCrews { get; set; } = new List<Models.Crew>();
        public List<Models.Permit> Permits { get; set; } = new List<Models.Permit>();
        public decimal TotalProjectCost { get; set; }
        public int CompletedTasksCount { get; set; }
        public int PendingTasksCount { get; set; }
        public List<ConflictAlert> ProjectConflicts { get; set; } = new List<ConflictAlert>();
    }

    public class ConflictAlert
    {
        public string Type { get; set; } = string.Empty; // Schedule, Resource, Budget, Permit
        public string Severity { get; set; } = string.Empty; // Low, Medium, High, Critical
        public string Message { get; set; } = string.Empty;
        public DateTime DetectedDate { get; set; } = DateTime.Now;
        public int? RelatedTaskID { get; set; }
        public string? RelatedTaskName { get; set; }
    }

    public class SimulationResultViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<ConflictAlert> Conflicts { get; set; } = new List<ConflictAlert>();
        public decimal? CostImpact { get; set; }
        public int? ScheduleImpactDays { get; set; }
        public DateTime? NewProjectEndDate { get; set; }
    }

    public class ContactFormViewModel
    {
        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone Number is required")]
        [StringLength(25, ErrorMessage = "Phone Number cannot exceed 25 characters")]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(120, ErrorMessage = "Subject cannot exceed 120 characters")]
        public string? Subject { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [StringLength(1500, ErrorMessage = "Message cannot exceed 1500 characters")]
        public string Message { get; set; } = string.Empty;
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}