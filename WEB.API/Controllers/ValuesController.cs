using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WEB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        [Authorize]
        public IActionResult GetValues()
        {
            var values = new[] {"Arifur", "Rahman", "sazal" };

            return Ok(values);
        }

        // GET api/values/1
        [Authorize(Roles = "User")]
        [HttpGet("{id}")]
        public IActionResult GetValue(int id)
        {
            var values = new[] { "Arifur", "Rahman", "sazal" };

            var value = values[id];

            return Ok(value);
        }

        // GET api/values/admin
        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult AdminValue()
        {

            var value = "I am admin";

            return Ok(value);
        }
    }
}
