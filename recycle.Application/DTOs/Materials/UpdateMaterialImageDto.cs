using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.Materials
{
    public class UpdateMaterialImageDto
    {
        public IFormFile? Image { get; set; }

    }
}
