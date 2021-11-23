using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecNetLogin
{
    public class LogInResult
    {
        public bool Success { get; set; }
        public string Auth { get; set; }
        public string Error { get; set; }
    }
}
