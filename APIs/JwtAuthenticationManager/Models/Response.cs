using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace JwtAuthenticationManager.Models
{
    public class Response
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public string Data { get; set; }
    }
}
