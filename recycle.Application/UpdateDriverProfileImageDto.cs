using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application
{
    public record UpdateDriverProfileImageDto
    {
        public string? ImageUrl { get; set; }
        public string? ImageLocalPath { get; set; }
        public IFormFile Image { get; set; }
    }
}
