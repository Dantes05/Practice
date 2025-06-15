using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Extensions
{
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }
}
