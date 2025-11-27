using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application
{
    public record DriverProfileDto
    {
        public string stringUserId { get; set; }
        public string IdNumber { get; set; }
        public string? ImageUrl { get; set; } = null;
        public string? ImageLocalPath { get; set; } = null;
        public IFormFile Image {  get; set; }
    }
}
