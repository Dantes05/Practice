using Application.Validators;
using System.ComponentModel.DataAnnotations;


namespace Application.DTOs
{
    public class UpdateTaskDto
    {
        [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters")]
        public string? Title { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters")]
        public string? Description { get; set; }

        public string? Priority { get; set; }

        [Required]
        [FutureDate(ErrorMessage = "Due date must be in the future")]
        public DateTime? DueDate { get; set; }

        public string? AssigneeId { get; set; }
    }
}