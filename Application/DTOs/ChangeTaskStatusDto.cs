using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class ChangeTaskStatusDto
    {
        [Required(ErrorMessage = "Status is required")]
        public TaskaStatus Status { get; set; }
    }
}
