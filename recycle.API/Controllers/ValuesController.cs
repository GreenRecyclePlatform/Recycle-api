using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace recycle.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles ="Admin,User")]
        public ActionResult<string> Get()
        {
            return Ok("Authentication and Authroization are working...");
        }
    }
}
