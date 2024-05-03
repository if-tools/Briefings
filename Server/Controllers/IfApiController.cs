using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace IFToolsBriefings.Server.Controllers
{
    [Route("api/if/[controller]")]
    [ApiController]
    public class IfApiController : Controller
    {
        [HttpGet("[action]")]
        public async Task<ActionResult<string>> GetFlightFpl(string callSign)
        {
            using var http = new HttpClient();
            
            return await http.GetStringAsync($"http://{Environment.GetEnvironmentVariable("IF_TOOLS_ADDRESS")}/api/IfApi/GetFlightFpl?callSign={callSign}");
        }
    }
}