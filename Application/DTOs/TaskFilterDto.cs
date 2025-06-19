using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class TaskFilterDto
    {
        public TaskaStatus? Status { get; set; }
        public TaskPriority? Priority { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? DueDateFrom { get; set; } 
        public DateTime? DueDateTo { get; set; }  
        public string? AssigneeId { get; set; }
        public string? CreatorId { get; set; }
        public string? SortBy { get; set; }    
        public bool? SortDescending { get; set; } 
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
