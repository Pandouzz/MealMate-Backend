using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;

[ApiController]
[Route("api/[controller]")]
public class RegisterController : ControllerBase 
{
    [HttpPost]
    public IActionResult Register([FromBody] RegisterRequest request) 
    {
        try
        {
            var result = RegistrationManager.Register(request);

            if (!result.Success)
            {
                if (result.ErrorCode == RegistrationError.EmailAlreadyExists)
                    return Conflict(new { success = false, message = "E-Mail ist bereits registriert." });

                return BadRequest(new { success = false, message = result.ErrorMessage });
            }

            Console.WriteLine($"Registrierung OK für {request.Email}, neue UserId: {result.UserId}");
            return Ok(new { success = true, userId = result.UserId });
        }
        catch (OracleException ox) when (ox.Number == 1) // ORA-00001: unique constraint violated
        {
            Console.WriteLine("DB-CONSTRAINT-ERROR: " + ox.Message);
            return Conflict(new { success = false, message = "E-Mail ist bereits registriert." });
        }
        catch (OracleException ox)
        {
            Console.WriteLine("DB-ERROR: " + ox);
            return StatusCode(500, new { success = false, message = "Datenbankfehler." });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unhandled error in Register: " + ex);
            return StatusCode(500, new { success = false, message = "Interner Serverfehler." });
        }   
    }
}

public class RegisterRequest 
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Email { get; set; }

    public string? PwHash { get; set; }

    public string? ImageUrl { get; set; }    

    public string? Salt { get; set; }
    
    public string? Password { get; set; }
}