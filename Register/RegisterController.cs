using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class RegisterController : ControllerBase 
{
    [HttpPost]
    public IActionResult Register([FromBody] RegisterRequest request) 
    {
        var result = RegistrationManager.Register(request);

        if(!result.Success && result.ErrorCode == RegistrationError.EmailAlreadyExists)
            return Conflict(new { success = false, message = "E-Mail ist bereits registriert" });

        if(!result.Success)
            return BadRequest(new { success = false, message = result.ErrorMessage });

        Console.WriteLine($"Registrierung OK für {request.Email}, neue UserId: {result.UserId}");
        return Ok(new { success = true, userId = result.UserId });    
    }
}

public class RegisterRequest 
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Email { get; set; }

    public string PwHash { get; set; }

    public string? ImageUrl { get; set; }    

    public string? Salt     { get; set; }
}