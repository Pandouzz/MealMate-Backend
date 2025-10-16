using System.Text.RegularExpressions;

internal static class RegistrationManager 
{
     public static RegisterResult Register(RegisterRequest req)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(req.Email) || req.Email.Length > 254)
                return RegisterResult.Fail("Ungültige E-Mail.");

            
            if (!Regex.IsMatch(req.FirstName ?? "", @"^[\p{L}\p{M}' \-]{1,50}$"))
                return RegisterResult.Fail("Ungültiger Vorname.");

            if (string.IsNullOrWhiteSpace(req.Password) && string.IsNullOrWhiteSpace(req.PwHash))
                return RegisterResult.Fail("Passwort (oder übergangsweise PwHash) ist erforderlich.");

            if (UserStore.EmailExists(req.Email))
                return RegisterResult.Fail("E-Mail existiert bereits.", RegistrationError.EmailAlreadyExists);

            string finalHash;
            string finalSalt;

            if (!string.IsNullOrWhiteSpace(req.Password))
            {
                var result = PasswordHasher.ComputeHash(req.Password);
                finalHash = result.Hash;
                finalSalt = result.Salt;
            }
            else
            {
                finalHash = req.PwHash!;
                finalSalt = string.IsNullOrWhiteSpace(req.Salt)
                    ? PasswordHasher.GenerateSalt()     
                    : req.Salt!;
            }

            var user = new User
            {
                FirstName = req.FirstName,
                LastName  = req.LastName,
                Email     = req.Email,
                PwHash    = finalHash,
                ImageUrl  = req.ImageUrl,
                Salt      = finalSalt
            };

            int newId = UserStore.CreateUser(user);

            if (newId == -1)
                return RegisterResult.Fail("E-Mail existiert bereits.", RegistrationError.EmailAlreadyExists);

            return RegisterResult.Ok(newId);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Registrierung fehlgeschlagen: " + ex);
            return RegisterResult.Fail("Unerwarteter Fehler.");
        }
    }
}

internal enum RegistrationError { None = 0, EmailAlreadyExists = 1 }

internal readonly struct RegisterResult
{
    public bool Success { get; }
    public int UserId { get; }
    public string ErrorMessage { get; }
    public RegistrationError ErrorCode { get; }

    private RegisterResult(bool success, int userId, string error, RegistrationError code)
    {
        Success = success; UserId = userId; ErrorMessage = error; ErrorCode = code;
    }

    public static RegisterResult Ok(int id) => new RegisterResult(true, id, "", RegistrationError.None);
    public static RegisterResult Fail(string msg, RegistrationError code = RegistrationError.None)
        => new RegisterResult(false, 0, msg, code);   
}