using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class TaskHistoryDto
    {
        public string Id { get; set; }
        public string ChangedField { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public DateTime ChangedAt { get; set; }
        public string TaskaId { get; set; }
        public string ChangedById { get; set; }
    }
}
