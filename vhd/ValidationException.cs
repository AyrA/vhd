using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vhd
{
    public class ValidationException : Exception
    {
        public string ParameterName { get; private set; }

        public ValidationException(string ParamName, string Message) : base(Message)
        {
            ParameterName = ParamName;
        }

        public ValidationException(string ParamName, string Message, Exception InnerException) : base(Message, InnerException)
        {
            ParameterName = ParamName;
        }

    }
}
