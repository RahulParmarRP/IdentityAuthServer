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
        [HttpGet("test")]
        [Authorize(Policy = "AuthorizeByApiScope")]
        public ActionResult GetTestData()
        {
            return Ok("this is test data");
        }

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

        [HttpGet("admin")]
        [Authorize(Policy = "AdminSecure")]
        public ActionResult UpdateAdminData()
        {
            return Ok("admin has updated the data");
        }

        [HttpGet("scope")]
        [Authorize(Policy = "AuthorizeByApiScope")]
        public ActionResult AuthorizeByApiScope()
        {
            return Ok("AuthorizeByApiScope");
        }
    }
}