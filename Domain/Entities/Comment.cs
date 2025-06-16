using System;

namespace Domain.Entities
{
    public class Comment
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string AuthorId { get; set; }
        public User Author { get; set; }

        public string TaskaId { get; set; }
        public Taska Taska { get; set; }
    }
}