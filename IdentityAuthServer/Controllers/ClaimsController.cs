using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IdentityAuthServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClaimsController : ControllerBase
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
        [Route("getClaims")]
        //[Authorize(IdentityServerConstants.LocalApi.PolicyName)]
        [Authorize(Policy = "ApiScope")]
        //[Authorize(Policy = "PublicSecure")]
        //[Authorize(Policy = "AuthorizeByApiScope")]
        //[Authorize(Policy = "UserSecure")]
        public IActionResult TestUserClaims()
        {
            var claims = from c in User.Claims
                         select new
                         {
                             c.Type,
                             c.Value
                         };

            return new JsonResult(claims);
        }

        [Route("userClaims")]
        //[Authorize(IdentityServerConstants.LocalApi.PolicyName)]
        public IActionResult UserClaims()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToArray();
            return Ok(new { message = "Hello API", claims });
        }
    }
}