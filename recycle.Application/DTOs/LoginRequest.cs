using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs
{
    public class LoginRequest
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
       
    }
}
