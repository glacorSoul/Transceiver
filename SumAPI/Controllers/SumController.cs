using Microsoft.AspNetCore.Mvc;

namespace SumAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SumController : ControllerBase
    {
        [HttpGet()]
        public int Get()
        {
            return 42;
        }

        [HttpGet("{a}/{b}")]
        public int Get([FromRoute]int a, [FromRoute] int b)
        {
            return a + b;
        }
    }
}
