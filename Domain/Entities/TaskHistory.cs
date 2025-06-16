using System;

namespace Domain.Entities
{
    public class TaskHistory
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ChangedField { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        public string TaskaId { get; set; }
        public Taska Taska { get; set; }

        public string ChangedById { get; set; }
        public User ChangedBy { get; set; }
    }
}