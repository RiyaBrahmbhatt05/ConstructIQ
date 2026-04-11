using ConstructionSimulator.Models;
using Microsoft.AspNetCore.Mvc;

namespace ConstructionSimulator.Controllers
{
    public class ChatController : Controller
    {
        [HttpPost]
        public IActionResult Ask([FromBody] ChatRequest request)
        {
            return BadRequest(new { error = "Chat feature is currently disabled." });
        }
    }
}