using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class CreateCommentDto
    {
        public string Text { get; set; }
        public string TaskaId { get; set; }
    }
}
