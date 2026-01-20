using System.Numerics;
using System.Runtime.InteropServices.JavaScript;
using GoWheels.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GoWheels.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Test : ControllerBase
    {
        private readonly IPostsService _postsService;

        public Test(IPostsService postsService)
        {
            _postsService = postsService;
        }

        /** Experiment 1: Just Testing **/

        // GET: api/<Test>
        [HttpGet]
        public string Get()
        {
            return "Just open '/Controllers/Test.cs' and manipulate the Methods and the Routes";
        }

        // GET api/<Test>/5
        [HttpGet("{id}")]
        public string Get(BigInteger id)
        {
            return $"Identity_function({id}) = {id}";
        }
        [HttpGet("square/{x}")]
        public string Square(BigInteger x)
        {
            return $"Square_function({x}) = {x * x}";
        }

        /** Experiment 2: Testing PostsService **/

        // GET: api/test/recent
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent()
        {
            // Using the injected service to get data
            var posts = await _postsService.GetRecentPostsAsync(3);
            return Ok(posts);
        }
        
        // POST api/<Test>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<Test>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<Test>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}