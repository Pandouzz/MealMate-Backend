using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]

public class ProfileController : ControllerBase
{
    [HttpPost]

    public IActionResult LoadProfile([FromBody] string requestedEmail)
    {
        User queriedDbUser = UserStore.TraceUser(requestedEmail);
        if (queriedDbUser == null)
        {
            Console.WriteLine("Nutzer konnte nicht gefunden werden!");
            return NotFound();
        }
        return Ok(queriedDbUser);
    }
}