using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TestWebAPIPolicyBasedAuthorization.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestPolicyAuthController : ControllerBase
    {
        [Authorize(Policy = "PublicSecure")]
        [HttpGet("public")]
        public ActionResult GetPublicData()
        {
            return Ok("this is public data");
        }

        [Authorize(Policy = "UserSecure")]
        [HttpGet("user")]
        public ActionResult GetUserData()
        {
            return Ok("this is user data");
        }

        [Authorize(Policy = "AdminSecure")]
        [HttpGet("admin")]
        public ActionResult UpdateAdminData()
        {
            return Ok("admin has updated the data");
        }
    }
}