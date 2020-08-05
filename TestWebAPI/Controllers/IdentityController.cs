using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TestWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AuthorizeByApiScope")]
    public class IdentityController : ControllerBase
    {
        [HttpGet]
        [Route("test")]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
            })
            .ToArray();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("anonymousTest")]
        public IActionResult GetData()
        {
            return Ok("anonymous test success");
        }

        [HttpGet]
        [Route("claims")]
        public IActionResult GetClaims()
        {
            var claims = from c in User.Claims
                         select new
                         {
                             c.Type,
                             c.Value
                         };

            return new JsonResult(claims);
        }

        [HttpGet]
        [Route("enduser")]
        [Authorize(Policy = "UserSecure")]
        public IActionResult GetClaimsEndUser()
        {
            var claims = from c in User.Claims
                         select new
                         {
                             c.Type,
                             c.Value
                         };

            return new JsonResult(claims);
        }

        [HttpGet]
        [Route("clientadmin")]
        [Authorize(Policy = "AdminSecure")]
        public IActionResult GetClaimsClientAdminUser()
        {
            var claims = from c in User.Claims
                         select new
                         {
                             c.Type,
                             c.Value
                         };

            return new JsonResult(claims);
        }
    }
}