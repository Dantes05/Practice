using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    public class Taska
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string CreatorId { get; set; }
        public User Creator { get; set; }

        public string? AssigneeId { get; set; }
        public User? Assignee { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<TaskHistory> History { get; set; } = new List<TaskHistory>();
    }
}